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
		selectedLanguage = settingsService.Language;
	}

	partial void OnSelectedLanguageChanged(string value) {
		settingsService.Language = SelectedLanguage;
	}
}
