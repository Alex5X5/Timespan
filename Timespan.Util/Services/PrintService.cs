namespace Timespan.Util.Services;

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;

public class PrintService {

	public PrintService() {
		
	}

	public void Print(string path) {
		var pi = new ProcessStartInfo("C:\file.docx");
		pi.UseShellExecute = true;
		pi.Verb = "print";
		pi.Arguments = "PATH_TO_PRINTER";
		var process = System.Diagnostics.Process.Start(pi);

	}
}
