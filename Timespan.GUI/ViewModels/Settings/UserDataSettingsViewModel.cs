using CommunityToolkit.Mvvm.ComponentModel;

using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Settings;

public partial class UserDataSettingsViewModel : ViewModelBase, ISettingsViewChild {

	private SettingsService settingsService;

	[ObservableProperty]
	private string usernameTextboxText = "";

	[ObservableProperty]
	private string startDateTextboxText = "";

	[ObservableProperty]
	private string jobNameTextboxText = "";

	public UserDataSettingsViewModel(DateTimeService dateTimeService, SettingsService settingsService) : base() {
		this.settingsService = settingsService;
		UsernameTextboxText = settingsService.Username;
		StartDateTextboxText = settingsService.StartDateString;
		JobNameTextboxText = settingsService.JobName;
	}

	partial void OnJobNameTextboxTextChanged(string value) {
		settingsService.JobName = JobNameTextboxText;
	}

	partial void OnStartDateTextboxTextChanged(string value) {
		settingsService.StartDateString = StartDateTextboxText;
	}

	partial void OnUsernameTextboxTextChanged(string value) {
		settingsService.Username = UsernameTextboxText;
	}

	public void OnUnload() {
	}
}
