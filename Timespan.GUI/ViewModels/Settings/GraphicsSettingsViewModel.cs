namespace Timespan.GUI.ViewModels.Settings;

using CommunityToolkit.Mvvm.ComponentModel;

using Timespan.GUI.Services;
using Timespan.Util.Services;

public partial class GraphicsSettingsViewModel : ViewModelBase, ISettingsViewChild {

	private SettingsService settingsService;

	[ObservableProperty]
	private string selectedTheme = "";
	public List<string> AvailableThemes { get; }

	public GraphicsSettingsViewModel(SettingsService settingsService) : base() {
		this.settingsService = settingsService;
		AvailableThemes = ThemeService.Singleton.AvailableThemes;
		SelectedTheme = settingsService.Theme;
		settingsService.OnPreSettingsSave += OnPreSettingsSave;
	}

	private void OnPreSettingsSave() {
		settingsService.Theme = SelectedTheme;
	}
}
