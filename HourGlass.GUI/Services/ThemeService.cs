namespace Hourglass.GUI.Services;

using Avalonia;
using Avalonia.Markup.Xaml.Styling;

public class ThemeService {

	public static readonly ThemeService Singleton;

	public readonly Dictionary<string, Uri> Themes;

	public List<string> AvailableThemes => Themes.Keys.ToList();

	static ThemeService() {
		Singleton = new ThemeService();
	}

	private ThemeService() {
		Themes = new() {
			{ "Timespan", new Uri("avares://Hourglass.GUI/Assets/Themes/TimespanTheme.axaml") },
			{ "TimespanDark", new Uri("avares://Hourglass.GUI/Assets/Themes/TimespanDarkTheme.axaml") },
			{ "Srh", new Uri("avares://YourApp/Hourglass.GUI/Themes/SrhTheme.axaml") },
			{ "SrhDark", new Uri("avares://YourApp/Hourglass.GUI/Themes/SrhDarkTheme.axaml") },
		};
	}

	public void ApplyTheme(string themeName) {
		if (!Themes.TryGetValue(themeName, out var themeUri))
			return;
		var mergedDicts = Application.Current!.Resources.MergedDictionaries;
		var existing = mergedDicts
			.OfType<ResourceInclude>()
			.FirstOrDefault(r => r.Source != null && Themes.Values.Contains(r.Source));
		if (existing != null)
			mergedDicts.Remove(existing);
		mergedDicts.Add(new ResourceInclude(themeUri) {
			Source = themeUri
		});
	}
}
