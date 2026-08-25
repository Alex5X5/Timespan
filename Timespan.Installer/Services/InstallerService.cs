using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Timespan.Util.Services;

public class InstallerService {

	public static bool IsInstalled() {
        string expetedPath = Path.Combine(PathService.APP_DATA_DIRECTORY, PathService.GetMainEntryPointFileName());
        string actualPath = PathService.GetMainEntryPointPath();
		return expetedPath.Equals(actualPath);
    }

	public static async Task InstallAsync(IProgress<int>? progress) {
		//return;
		try {
            string filePath = PathService.AppDataFilesPath("last_exec_path.txt");
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            if (!File.Exists(filePath))
                File.Create(filePath);
            progress?.Report(10);
            using (FileStream file = File.OpenWrite(filePath))
            using (StreamWriter writer = new(file)) {
                await writer.WriteAsync(PathService.GetMainEntryPointPath());
                await writer.FlushAsync();
            }
			string targetExePath = await CopyApplicationAndCreateShortcutAsync(progress);
			//Thread.Sleep(1000);
			//         Console.WriteLine($"Starting Process from File:{targetExePath}");
			Process.Start(targetExePath);
			Environment.Exit(0);
		} catch (Exception ex) {
			Console.WriteLine(ex);
		}
    }

	public static void ClearInstallDirectoryLeftovers() {
        Console.WriteLine("clear leftovers");
        string infoFilePath = PathService.AppDataFilesPath("last_exec_path");
		if (File.Exists(infoFilePath)) {
			string[] lines = File.ReadAllLines(infoFilePath);
			string oldExecutionDirectory = Path.GetDirectoryName(lines[0]);
			if (File.Exists(oldExecutionDirectory)) {
				File.Delete(oldExecutionDirectory);
			}
			File.Delete(infoFilePath);
        }
    }

	private static void CopyApplicationAndCreateShortcut() =>
		CopyApplicationAndCreateShortcutAsync(null).RunSynchronously();
	
    private static async Task<string> CopyApplicationAndCreateShortcutAsync(IProgress<int>? progress) {
		string targetDirectory = PathService.APP_DATA_DIRECTORY;
		string targetExePath = PathService.AppDataFilesPath(PathService.GetMainEntryPointFileName());
        try {
			// Create the directory if it doesn't exist
			if (!Directory.Exists(targetDirectory)) {
				Directory.CreateDirectory(targetDirectory);
				Console.WriteLine($"Created directory: {targetDirectory}");
			}

			using (StreamWriter writer = new StreamWriter(PathService.DesktopPath("Hourglass.exe.url"))) {
				writer.WriteLine("[InternetShortcut]");
				writer.WriteLine("URL=file:///" + targetExePath);
				writer.WriteLine("IconIndex=0");
				string icon = targetExePath.Replace('\\', '/');
				writer.WriteLine("IconFile=" + icon);
			}

			progress?.Report(20);

            // Copy the executable to the target location
            File.Copy(PathService.GetMainEntryPointPath(), targetExePath, overwrite: true);
			Console.WriteLine("Application copied successfully!");
			progress?.Report(50);
			return targetExePath;
		} catch (Exception ex) {
			Console.WriteLine($"Error copying application: {ex.Message}");
		}
		return "";
	}
}
