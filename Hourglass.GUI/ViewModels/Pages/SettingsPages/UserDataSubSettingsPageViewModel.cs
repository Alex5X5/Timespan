namespace Hourglass.GUI.ViewModels.Pages.SettingsPages;

using Hourglass.Util.Services;
using ReactiveUI;
using System.ComponentModel;

public partial class UserDataSubSettingsPageViewModel : SubSettingsPageViewModelBase {

	private string usernameTextboxText = "";
	public string UsernameTextboxText {
        set {
            if (value == null)
                return;
            if (value != usernameTextboxText)
                HasUnsavedChanges = true;
            this.RaiseAndSetIfChanged(ref usernameTextboxText, value);
        }
		get => usernameTextboxText;
	}

	private string startDateTextboxText = "";
	public string StartDateTextboxText {
        set {
            if (value == null)
                return;
            if (value != startDateTextboxText)
                HasUnsavedChanges = true;
            this.RaiseAndSetIfChanged(ref startDateTextboxText, value);
        }
        get => startDateTextboxText;
	}

	private string jobNameTextboxText = "";
	public string JobNameTextboxText {
        set {
            if (value == null)
                return;
            if (value != jobNameTextboxText)
                HasUnsavedChanges = true;
            this.RaiseAndSetIfChanged(ref jobNameTextboxText, value);
        }
        get => jobNameTextboxText;
	}

	public override string Title => TranslatorService.Singleton["Views.Pages.Settings.UserData.Title"] ?? "User Data";

	public new event PropertyChangedEventHandler? PropertyChanged;

	public UserDataSubSettingsPageViewModel() : this(null, null, null, null) {

	}

	public UserDataSubSettingsPageViewModel(DateTimeService dateTimeService, SettingsPageViewModel settingsController, MainViewModel pageController, SettingsService settingsService) : base(dateTimeService, pageController, settingsService) {
		if (settingsService != null) {
			settingsService.OnUsernameChanged += 
				val => this.RaiseAndSetIfChanged(ref usernameTextboxText, settingsService.Username);
			settingsService.OnStartDateChanged +=
				val => this.RaiseAndSetIfChanged(ref startDateTextboxText, settingsService.StartDateString);
			settingsService.OnJobNameChanged += 
				val => this.RaiseAndSetIfChanged(ref jobNameTextboxText, settingsService.JobName);
			settingsService.OnPreSettingsSave += () => {
			};
            this.RaiseAndSetIfChanged(ref usernameTextboxText, settingsService.Username);
            this.RaiseAndSetIfChanged(ref startDateTextboxText, settingsService.StartDateString);
            this.RaiseAndSetIfChanged(ref jobNameTextboxText, settingsService.JobName);
		}
	}

	public void OnLoad() {
		Console.WriteLine("loading User Data Sub Settings Page!");
    }

    public override void SaveSettings() {
        settingsService.StartDateString = StartDateTextboxText;
        settingsService.Username = UsernameTextboxText;
        settingsService.JobName = JobNameTextboxText;
    }
}