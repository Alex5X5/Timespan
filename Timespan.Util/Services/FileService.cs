namespace Timespan.Util.Services;

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;


public static class FileService {

	public static unsafe char* DecodeBufferAnsi(byte* buffer, int fileSize, out int charCount) {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding encoding = Encoding.GetEncoding(1252);
		int bytesPerChar = encoding.GetByteCount(['a']);
		charCount = fileSize / bytesPerChar;
		char* chars = (char*)NativeMemory.Alloc((uint)(charCount * sizeof(char)));
		charCount = encoding.GetChars(buffer, fileSize, chars, charCount);
		return chars;
	}

	public static unsafe byte* EncodeBufferAnsi(char* chars, int charCount, out int fileSize) {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding encoding = Encoding.GetEncoding(1252);
        fileSize = charCount * sizeof(char);
        byte* buffer = (byte*)NativeMemory.Alloc((uint)fileSize);
		fileSize = encoding.GetBytes(chars, charCount, buffer, fileSize);
		return buffer;
	}

	public static unsafe byte* LoadFileUnsafe(string path, out int fileSize) {
		fileSize = (int)new FileInfo(path).Length;
		byte* file = (byte*)NativeMemory.Alloc((uint)fileSize);
		using FileStream fs = new(path, FileMode.Open, FileAccess.Read);
		fs.ReadExactly(new Span<byte>(file, fileSize));
		return file;
	}

	public static unsafe void WriteFileUnsafe(byte* content, string pathName, int size) {
		if (!Directory.Exists(Path.GetDirectoryName(pathName)))
			Directory.CreateDirectory(Path.GetDirectoryName(pathName));
		using FileStream fileHandle = File.OpenWrite(pathName);
		fileHandle.Write(new Span<byte>(content, size));
	}
}
