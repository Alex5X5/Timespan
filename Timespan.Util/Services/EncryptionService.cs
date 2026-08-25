namespace Timespan.Util.Services; 

using Timespan.Util.Services.Interfaces;

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

public class EncryptionService : IEncryptionService {
	private byte[] _key;

	private static readonly int KeySize = 32; // 256-bit key
	private static readonly int IVSize = 16;  // 128-bit IV (Initialization Vector)
	private static readonly int BlockSize = 16; // AES block size
	
	public EncryptionService(string password) {
		_key = new byte[KeySize];
		DeriveKeyFromPassword(password);
	}

	private void DeriveKeyFromPassword(string password, byte[]? salt = null, int iterations = 100000) {
		if (salt == null) {
			salt = new byte[16];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(salt);
		}

		using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
		_key = pbkdf2.GetBytes(KeySize);
	}

	public unsafe byte* EncryptBuffer(byte* inputData, int inputLength, out int outputBufferSize) {
		outputBufferSize = inputLength + IVSize + BlockSize;
		byte* result = (byte*)NativeMemory.Alloc((uint)outputBufferSize);
		using (Aes aes = Aes.Create()) {
			aes.KeySize = KeySize * 8;
			aes.Key = _key;
			aes.GenerateIV();
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;

			// Write IV to the beginning of output
			Marshal.Copy(aes.IV, 0, (IntPtr)result, IVSize);

			using (var encryptor = aes.CreateEncryptor()) {
				using (var inputStream = new UnmanagedMemoryStream(inputData, inputLength))
				using (var outputStream = new UnmanagedMemoryStream(result + IVSize, outputBufferSize - IVSize, outputBufferSize - IVSize, FileAccess.Write))
				using (var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write)) {
					// Process in chunks to avoid loading everything into memory
					byte[] buffer = new byte[64 * 1024]; // 64KB chunks
					int totalRead = 0;
					int bytesRead;

					while (totalRead < inputLength && (bytesRead = inputStream.Read(buffer, 0, Math.Min(buffer.Length, inputLength - totalRead))) > 0) {
						cryptoStream.Write(buffer, 0, bytesRead);
						totalRead += bytesRead;
					}

					cryptoStream.FlushFinalBlock();

					// Return total size: IV + encrypted data
					//outputBufferSize = IVSize + (int)outputStream.Position;
				}
			}
		}
		return result;
	}

	public unsafe byte* DecryptBuffer(byte* encryptedData, int encryptedLength, out int outputBufferSize) {
		outputBufferSize = encryptedLength - IVSize - BlockSize;
		byte* result = (byte*)NativeMemory.Alloc((uint)outputBufferSize);
		using (Aes aes = Aes.Create()) {
			aes.KeySize = KeySize * 8;
			aes.Key = _key;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;

			// Extract IV from beginning of encrypted data
			byte[] iv = new byte[IVSize];
			Marshal.Copy((IntPtr)encryptedData, iv, 0, IVSize);
			aes.IV = iv;

			using (var decryptor = aes.CreateDecryptor()) {
				// Input stream starts after IV
				using (var inputStream = new UnmanagedMemoryStream(encryptedData + IVSize, encryptedLength - IVSize))
				using (var outputStream = new UnmanagedMemoryStream(result, outputBufferSize, outputBufferSize, FileAccess.Write))
				using (var cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read)) {
					byte[] buffer = new byte[64 * 1024]; // 64KB chunks
					int totalWritten = 0;
					int bytesRead;

					while ((bytesRead = cryptoStream.Read(buffer, 0, buffer.Length)) > 0) {
						if (totalWritten + bytesRead > outputBufferSize)
							throw new ArgumentException("Output buffer too small");
						outputStream.Write(buffer, 0, bytesRead);
						totalWritten += bytesRead;
					}
				
					outputBufferSize = (int)outputStream.Position - IVSize;
				}
			}
		}
		return result;
	}

	public void EncryptFile(string path) {
		unsafe {
			byte* loadedFile = FileService.LoadFileUnsafe(path, out int fileSize);
			byte* encryptedData = EncryptBuffer(loadedFile, fileSize, out int encryptedBufferSize);
			NativeMemory.Free(loadedFile);
			FileService.WriteFileUnsafe(encryptedData, path, encryptedBufferSize);
			NativeMemory.Free(encryptedData);
		}
	}

	public void DecryptFile(string path) {
		unsafe {
			byte* loadedFile = FileService.LoadFileUnsafe(path, out int fileSize);
			byte* encryptedData = DecryptBuffer(loadedFile, fileSize, out int encryptedBufferSize);
			NativeMemory.Free(loadedFile);
			FileService.WriteFileUnsafe(encryptedData, path, encryptedBufferSize);
			NativeMemory.Free(encryptedData);
		}
	}
}
