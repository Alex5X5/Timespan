namespace Timespan.GUI.ViewModels.Settings;

using System.Diagnostics;
using System.Runtime.InteropServices;

using Timespan.Util.Services;

public partial class AboutSettingsViewModel : ViewModelBase, ISettingsViewChild {

	public AboutSettingsViewModel() : base() {
	}

	internal static void OnAvaloniaButtonClick() {
		OpenUrl("https://avaloniaui.net");
	}

	internal static void OnEmailButtonClick() {
		OpenUrl("mailto://support.timespan@gmail.com");
	}

	internal static void OnVisualStudioButtonClick() {
		OpenUrl("https://visualstudio.microsoft.com");
	}

	internal static void OnDotnetButtonClick() {
		OpenUrl("https://visualstudio.microsoft.com");
	}

	public static void OnFigmaButtonClick() {
		OpenUrl("https://www.figma.com");
	}

	internal static void OnIllustratorButtonClick() {
		OpenUrl("https://www.adobe.com/de/products/illustrator.html");
	}

	internal static void OnGithubButtonClick() {
		OpenUrl("https://github.com/Alex5X5/Timespan");
	}

	internal static void OnKofiButtonClick() {
		OpenUrl("https://Ko-fi.com/timespan");
	}

	internal static void OnSrhButtonClick() {
		OpenUrl("https://www.srh-bbw-dresden.de");
	}

	private static void OpenUrl(string url) {
		try {
			Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
		} catch {
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				url = url.Replace("&", "^&");
				Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
			} else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
				Process.Start("xdg-open", url);
			} else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
				Process.Start("open", url);
			} else {
				throw;
			}
		}
	}
}
