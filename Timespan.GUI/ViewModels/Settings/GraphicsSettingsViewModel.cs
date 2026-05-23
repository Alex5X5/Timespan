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
		settingsService.OnPreSettingsSave += OnPreSettingsSave;
		SelectedTheme = settingsService.Theme;
	}

	private void OnPreSettingsSave() {
		settingsService.Theme = SelectedTheme;
	}

	public void OnUnload() {
		settingsService.OnPreSettingsSave -= OnPreSettingsSave;
	}
}
