namespace Timespan;

using Avalonia;

using Timespan.GUI;
using Timespan.Util.Services;

public class Program {
	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	[STAThread]
	public static void Main(string[] args) {

		//PathService.PrintDetailedInfo();
		PathService.ExtractFiles("Timespan");

		//PrintService ps = new PrintService();
		//ps.Print(PathService.AssetsPath("output-readable-indexers.pdf"));
		try {
			BuildAvaloniaApp()
				.StartWithClassicDesktopLifetime(args);
		} catch (Exception ex) {
			string path = PathService.CrashesPath($"crash-{DateTimeService.ToDayAndMonthAndYearString(DateTime.Now)}.log");
			File.WriteAllText(path, ex.ToString());
		}
		//EncryptionService service = new("test"); 
		//service.EncryptFile(PathService.FilesPath("database"));
	}

	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();
}