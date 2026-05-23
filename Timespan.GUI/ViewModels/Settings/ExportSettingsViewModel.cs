using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Settings;

public class ExportSettingsViewModel : ViewModelBase, ISettingsViewChild {

	private SettingsService settingsService;

	public ExportSettingsViewModel(SettingsService settingsService) : base() {
		this.settingsService = settingsService;
		settingsService.OnPreSettingsSave += OnPreSettingsSave;
	}

	private void OnPreSettingsSave() {
	}

	public void OnUnload() {
		settingsService.OnPreSettingsSave -= OnPreSettingsSave;
	}
}
