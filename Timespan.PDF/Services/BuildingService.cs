namespace Timespan.PDF.Services;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


public unsafe partial class PdfService {

	private void BufferValue(string indexName, string value) {
		BufferFieldValueUnsafe(indexName, value);
		BufferAnnotationValueUnsafe(indexName, value);
	}

	private void BufferFieldValueUnsafe(string indexName, string value) {
		InsertOperations[$"%%index-{indexName}-field"] = value;
	}

	private void BufferAnnotationValueUnsafe(string indexName, string value) {
		InsertOperations[$"%%index-{indexName}-annotation"] = value;
	}

	public char* BuildDocument(out int finalTextLength) {
		Console.WriteLine("started building document");
        finalTextLength = charCount;
		foreach (string key in InsertOperations.Keys)
			finalTextLength += InsertOperations[key].Length;
		Console.WriteLine($"final text length will be:{finalTextLength}");
		char* buffer = (char*)NativeMemory.AllocZeroed((uint)(finalTextLength * sizeof(char)));
		char* _buffer = buffer;
		foreach (string key in Indexers.Keys) {
			uint souceCharCopyCount = (uint)((char*)Indexers[key].Item2 - (char*)Indexers[key].Item1);
			uint sourceByteCopyCount = souceCharCopyCount * sizeof(char);
			NativeMemory.Copy((char*) Indexers[key].Item1, _buffer, sourceByteCopyCount);
			_buffer += souceCharCopyCount;
			InsertOperations.TryGetValue(key, out string? val);
			if (val != null) {
				string s = InsertOperations[key];
				if(s == "")
					continue;
				int charInsertCount = s.Length;
				uint byteInsertCount = (uint) (charInsertCount * sizeof(char));
				fixed (char* insert = &InsertOperations[key].ToCharArray()[0]) {
					//PrintCharsBefore(_buffer);
					//Console.WriteLine($"Inserting\'{val}\' at document position:{_buffer - buffer} at memory position:\'{(nuint)_buffer}\' for key:\'{key}\'");
					NativeMemory.Copy(insert, _buffer, byteInsertCount);
					_buffer += charInsertCount;
				}
			}
        }
        Console.WriteLine("finished building document");
        return buffer;
	}

	public ValueTuple<string, string, string>[] BuildTouples() {
		List<ValueTuple<string, string, string>> lines = [];
		foreach (string day in new List<string> { "monday", "tuesday", "wendsday", "thursday", "friday" }) {
			for(int i= 0; i<6; i++)
				lines.Add(BuildTuple(day, i));
		}
		return lines.ToArray();
	}

	private ValueTuple<string, string, string> BuildTuple(string dayName, int lineIndex) {
		string line = "";
		string hour = "";
		string range = "";
		if (InsertOperations.TryGetValue($"%%index-{dayName}_line_{lineIndex}", out string? line_))
			line = line_!;
		if (InsertOperations.TryGetValue($"%%index-{dayName}_hour_{lineIndex}", out string? hour_))
			hour = hour_!;
		if (InsertOperations.TryGetValue($"%%index-{dayName}_hour_range_{lineIndex}", out string? range_))
			range = range_!;
		return new ValueTuple<string, string, string>(line, hour, range);
	}
}
