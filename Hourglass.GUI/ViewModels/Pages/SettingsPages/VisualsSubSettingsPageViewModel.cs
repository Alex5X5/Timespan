using Hourglass.GUI.Services;
using ReactiveUI;

namespace Hourglass.GUI.ViewModels.Pages.SettingsPages;

public partial class VisualsSubSettingsPageViewModel : SubSettingsPageViewModelBase {

	public override string Title => TranslatorService.Singleton["Views.Pages.Settings.Visuals.Title"] ?? "Graphics Settings";

	private string selectedTheme = "";
	public string SelectedTheme {
        set {
            if (value == null)
                return;
            if (value != selectedTheme)
                HasUnsavedChanges = true;
            this.RaiseAndSetIfChanged(ref selectedTheme, value);
        }
        get => selectedTheme;
	}
	public List<string> AvailableThemes { get; set; }

	public VisualsSubSettingsPageViewModel() : this(null, null, null) {

	}

	public VisualsSubSettingsPageViewModel(DateTimeService dateTimeService, MainViewModel pageController, SettingsService settingsService) : base(dateTimeService, pageController, settingsService) {
        AvailableThemes = ThemeService.Singleton.AvailableThemes;
        if (settingsService != null) {
            settingsService.OnThemeChanged +=
                val => {
                    this.RaiseAndSetIfChanged(ref selectedTheme, settingsService.Theme);
                    ThemeService.Singleton.ApplyTheme(val);
                };
            this.RaiseAndSetIfChanged(ref selectedTheme, settingsService.Theme);
        }
	}

	public void OnLoad() {
		Console.WriteLine("loading Visuals Sub Settings Page!");
	}

	public override void SaveSettings() {
        settingsService.Theme = SelectedTheme;
    }
}