namespace Timespan.GUI.ViewModels.Settings;

using CommunityToolkit.Mvvm.ComponentModel;

using System.Linq;

using Timespan.Util.Services;

public partial class GeneralSettingsViewModel : ViewModelBase, ISettingsViewChild {

	private SettingsService settingsService;

	[ObservableProperty]
	private string selectedLanguage = "";
	public List<string> AvailableLanguages {
		get; set;
	}

	public GeneralSettingsViewModel(SettingsService settingsService) : base() {
		this.settingsService = settingsService;
		AvailableLanguages = TranslatorService.Singleton.AvailableTranslations.ToList();
		settingsService.OnPreSettingsSave += OnPreSettingsSave;
		SelectedLanguage = settingsService.Language;
	}

	private void OnPreSettingsSave() {
		settingsService.Language = SelectedLanguage;
	}

	public void OnUnload() {
		//settingsService.OnPreSettingsSave -= OnPreSettingsSave;
	}
}
