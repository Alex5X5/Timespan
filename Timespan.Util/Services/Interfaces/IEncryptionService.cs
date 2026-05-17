namespace Timespan.Util.Services.Interfaces;

public interface IEncryptionService {

	public unsafe byte* EncryptBuffer(byte* inputData, int inputLength, out int outputBufferSize);

	public unsafe byte* DecryptBuffer(byte* inputData, int inputLength, out int outputBufferSize);

	public unsafe void EncryptFile(string path);
	
	public unsafe void DecryptFile(string path);

}
