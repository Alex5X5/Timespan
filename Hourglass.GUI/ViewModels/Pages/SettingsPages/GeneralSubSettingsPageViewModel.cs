using ReactiveUI;

namespace Hourglass.GUI.ViewModels.Pages.SettingsPages;

public partial class GeneralSubSettingsPageViewModel : SubSettingsPageViewModelBase {
    
    private string selectedLanguage = "";
    public string SelectedLanguage {
        get => selectedLanguage;
        set {
            if (value == null)
                return;
            if (value != selectedLanguage)
                HasUnsavedChanges = true;
            this.RaiseAndSetIfChanged(ref selectedLanguage, value);
        }
    }
    public List<string> AvailableLanguages { get; set; }
    
    public override string Title => TranslatorService.Singleton["Views.Pages.Settings.General.Title"] ?? "General Settings";
        
    public GeneralSubSettingsPageViewModel() : this(null, null, null) {
    }
    
    public GeneralSubSettingsPageViewModel(DateTimeService dateTimeService, MainViewModel pageController, SettingsService settingsService) : base(dateTimeService, pageController, settingsService) {
        AvailableLanguages = TranslatorService.Singleton.AvailableTranslations.ToList();
        if (settingsService != null) {
            settingsService.OnLanguageChanged +=
                val => this.RaiseAndSetIfChanged(ref selectedLanguage, settingsService.Language);
            this.RaiseAndSetIfChanged(ref selectedLanguage, settingsService.Language);
        }
    }

	public void OnLoad() {
		Console.WriteLine("loading General Sub Settings Page!");
	}

    public override void SaveSettings() {
        settingsService.Language = selectedLanguage;
    }
}