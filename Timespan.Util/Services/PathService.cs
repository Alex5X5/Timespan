namespace Timespan.Util.Services;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

public static class PathService {

	public const int ASSETS_VERSION = 6;
	public const string APP_NAME = "Timespan";

	public static readonly string APP_DATA_DIRECTORY = Path.Combine(GetAppDataDirectory(), APP_NAME);
	public static readonly string ASSETS_DIRECTORY = Path.Combine(APP_DATA_DIRECTORY, "assets");
	public static readonly string LANGUAGES_DIRECTORY = Path.Combine(ASSETS_DIRECTORY, "lang");
	public static readonly string CRASHES_DIRECTORY = Path.Combine(APP_DATA_DIRECTORY, "logs");


	static PathService() {
		EnsurePathExists(APP_DATA_DIRECTORY);
		EnsurePathExists(ASSETS_DIRECTORY);
		EnsurePathExists(LANGUAGES_DIRECTORY);
		EnsurePathExists(CRASHES_DIRECTORY);
	}

	private static void EnsurePathExists(string path) {
		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);
	}

	private static bool RequireExtractAssets() {
		string fileName = AssetsPath("assets_version");
		if (!Directory.Exists(AssetsPath("")))
			Directory.CreateDirectory(AssetsPath(""));
		if (!File.Exists(fileName)) {
			File.WriteAllText(fileName, Convert.ToString(ASSETS_VERSION));
			return true;
		}
		string content = File.ReadAllText(fileName);
		try {
			var version = Convert.ToInt32(content);
			return version < ASSETS_VERSION;
		} catch(FormatException) {
			File.WriteAllText(fileName, Convert.ToString(ASSETS_VERSION));
			return true;
		} 
	}

    public static void ExtractFiles(string resourceNamespacePrefix) {
		if(!RequireExtractAssets())
			return;
		// Get executing assembly
		var assembly = Assembly.GetCallingAssembly();
		var resourceNames = assembly.GetManifestResourceNames();

		// Determine output path next to the running .exe
		string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

		for(int i=0; i<resourceNames.Length; i++) {
			// Filter resources inside the specified namespace
			if (!resourceNames[i].StartsWith(resourceNamespacePrefix))
				continue;
			string resourceName = resourceNames[i];
			resourceName = resourceName.Remove(0,resourceNamespacePrefix.Length + 1);
			while (resourceName.Split('.').Length>2) {
				resourceName = new StringBuilder(resourceName)
					.Insert(resourceName.IndexOf('.')+1, '\\')
						.Remove(resourceName.IndexOf('.'), 1)
							.ToString();
			}
			//resourceName= exeDirectory + resourceName;
			resourceName= FilesPath(resourceName);
			if (File.Exists(resourceName))
				continue;
            Directory.CreateDirectory(Path.GetDirectoryName(resourceName)!);
            using Stream? resourceStream = assembly.GetManifestResourceStream(resourceNames[i]);
            if (resourceStream == null) {
                Console.WriteLine($"Failed to load resource: {resourceName}");
                continue;
            }

            using FileStream fileStream = new FileStream(resourceName, FileMode.Create, FileAccess.Write);
            resourceStream.CopyTo(fileStream);
            Console.WriteLine($"Extracted: {resourceName}");

        }
	}

	public static string FilesPath(string fileName) =>
		Path.Combine(APP_DATA_DIRECTORY, fileName);

    public static string AssetsPath(string fileName) =>
		Path.Combine(ASSETS_DIRECTORY, fileName);

	public static string LanguagesPath(string fileName) =>
		Path.Combine(LANGUAGES_DIRECTORY, fileName);

	public static string CrashesPath(string fileName) =>
		Path.Combine(CRASHES_DIRECTORY, fileName);

	public static string DesktopPath(string fileName) =>
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), fileName);

    public static string GetMainEntryPointPath() {
		// Method 1: Try to get the entry assembly (most reliable for executable applications)
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		if (entryAssembly != null) {
			string assemblyPath = entryAssembly.Location;
			if (!string.IsNullOrEmpty(assemblyPath) && File.Exists(assemblyPath)) {
				return assemblyPath;
			}
		}

		// Method 2: Use the executing assembly as fallback
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		if (executingAssembly != null) {
			string assemblyPath = executingAssembly.Location;
			if (!string.IsNullOrEmpty(assemblyPath) && File.Exists(assemblyPath)) {
				return assemblyPath;
			}
		}

		// Method 3: Try getting from the current process (for .exe files)
		try {
			Process currentProcess = Process.GetCurrentProcess();
			string processPath = currentProcess.MainModule?.FileName;
			if (!string.IsNullOrEmpty(processPath) && File.Exists(processPath)) {
				return processPath;
			}
		} catch (Exception) {
			// Process.MainModule can throw exceptions in some contexts
		}

		// Method 4: Use AppDomain base directory as last resort
		string appDomainPath = AppDomain.CurrentDomain.BaseDirectory;
		if (!string.IsNullOrEmpty(appDomainPath)) {
			// Try to find the main executable in the base directory
			string[] possibleExtensions = { ".exe", ".dll" };
			string appName = AppDomain.CurrentDomain.FriendlyName;

			foreach (string ext in possibleExtensions) {
				string possiblePath = Path.Combine(appDomainPath,
					Path.ChangeExtension(appName, ext));
				if (File.Exists(possiblePath)) {
					return possiblePath;
				}
			}

			// If no specific file found, return the base directory
			return appDomainPath;
		}

		throw new InvalidOperationException("Unable to determine the main entry point path");
	}

	public static string GetMainEntryPointDirectory() =>
		Path.GetDirectoryName(GetMainEntryPointPath())!;

    public static string GetMainEntryPointFileName() =>
        Path.GetFileName(GetMainEntryPointPath());

    /// <summary>
    /// Alternative method that attempts to find the source file path (for development scenarios)
    /// Note: This only works in debug builds with specific compiler settings
    /// </summary>
    public static string GetSourceFilePath() {
		try {
			// Get the stack trace to find the calling method
			StackTrace stackTrace = new StackTrace(true);

			// Look for the Main method in the stack trace
			for (int i = 0; i < stackTrace.FrameCount; i++) {
				StackFrame frame = stackTrace.GetFrame(i);
				MethodBase method = frame.GetMethod();

				if (method != null && method.Name == "Main" &&
					method.DeclaringType != null) {
					string fileName = frame.GetFileName();
					if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName)) {
						return fileName;
					}
				}
			}
		} catch (Exception) {
			// Stack trace information might not be available
		}

		return null;
	}

	public static string GetAppDataDirectory() =>
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public static void PrintDetailedInfo() {
		Console.WriteLine("=== Main Entry Point Detection Details ===");

        Console.WriteLine($"Entry Point Path: {GetMainEntryPointPath() ?? "Not available"}");
        Console.WriteLine($"Entry Point Directory: {GetMainEntryPointDirectory() ?? "Not available"}");
        Console.WriteLine($"Entry Point File Name: {GetMainEntryPointFileName() ?? "Not available"}");

        // Entry Assembly
        Assembly entryAssembly = Assembly.GetEntryAssembly();
		Console.WriteLine($"Entry Assembly: {entryAssembly?.FullName ?? "Not available"}");
		Console.WriteLine($"Entry Assembly Location: {entryAssembly?.Location ?? "Not available"}");

		// Executing Assembly
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		Console.WriteLine($"Executing Assembly: {executingAssembly?.FullName ?? "Not available"}");
		Console.WriteLine($"Executing Assembly Location: {executingAssembly?.Location ?? "Not available"}");

		// Process Info
		try {
			Process currentProcess = Process.GetCurrentProcess();
			Console.WriteLine($"Process Name: {currentProcess.ProcessName}");
			Console.WriteLine($"Process Main Module: {currentProcess.MainModule?.FileName ?? "Not available"}");
		} catch (Exception ex) {
			Console.WriteLine($"Process Info Error: {ex.Message}");
		}

		// AppDomain Info
		Console.WriteLine($"AppDomain Base Directory: {AppDomain.CurrentDomain.BaseDirectory}");
		Console.WriteLine($"AppDomain Friendly Name: {AppDomain.CurrentDomain.FriendlyName}");

		// Source file (if available)
		string sourceFile = GetSourceFilePath();
		Console.WriteLine($"Source File Path: {sourceFile ?? "Not available (release build or no debug info)"}");
	}
}
