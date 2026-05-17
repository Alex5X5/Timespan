namespace Hourglass;

using Avalonia;
using Timespan.GUI;

public class Program {
	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	[STAThread]
	public static void Main(string[] args) {

  //      PathService.PrintDetailedInfo();
		//PathService.ExtractFiles("Hourglass");
		//PrintService ps = new PrintService();
		//ps.Print(PathService.AssetsPath("output-readable-indexers.pdf"));

		BuildAvaloniaApp()
			.StartWithClassicDesktopLifetime(args);

		//EncryptionService service = new("test"); 
		//service.EncryptFile(PathService.FilesPath("database"));
	}

	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();
}