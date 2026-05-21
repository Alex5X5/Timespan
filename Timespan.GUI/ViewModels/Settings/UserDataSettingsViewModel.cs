using CommunityToolkit.Mvvm.ComponentModel;

using Timespan.GUI.Services;
using Timespan.GUI.Types;
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
		settingsService.OnPreSettingsSave += () => {
			settingsService.Username = UsernameTextboxText;
			settingsService.StartDateString = StartDateTextboxText;
			settingsService.JobName = JobNameTextboxText;
		};
		UsernameTextboxText = settingsService.Username;
		StartDateTextboxText = settingsService.StartDateString;
		JobNameTextboxText = settingsService.JobName;
	}

	partial void OnUsernameTextboxTextChanged(string value) {
		settingsService.Username = value;
	}

	partial void OnStartDateTextboxTextChanged(string value) {
		settingsService.StartDateString = value;
	}

	partial void OnJobNameTextboxTextChanged(string value) {
		settingsService.JobName = value;
	}
}
