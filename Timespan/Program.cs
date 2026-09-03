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
		PathService.ExtractFiles("Timespan");
#if !DEBUG
		try {
#endif
		BuildAvaloniaApp()
				.StartWithClassicDesktopLifetime(args);
#if !DEBUG
		} catch (Exception ex) {
			string path = PathService.CrashesPath($"crash-{DateTimeService.ToDayAndMonthAndYearString(DateTime.Now)}.log");
			File.WriteAllText(path, ex.ToString());
		}
#endif
	}

	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();
}