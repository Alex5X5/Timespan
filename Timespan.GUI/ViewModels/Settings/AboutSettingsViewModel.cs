using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Settings;

public class AboutSettingsViewModel : ViewModelBase, ISettingsViewChild {

	private SettingsService settingsService;

	public AboutSettingsViewModel(SettingsService settingsService) : base() {
		this.settingsService = settingsService;
		settingsService.OnPreSettingsSave += OnPreSettingsSave;
	}

	private void OnPreSettingsSave() {
	}

	public void OnUnload() {
		settingsService.OnPreSettingsSave -= OnPreSettingsSave;
	}
}
