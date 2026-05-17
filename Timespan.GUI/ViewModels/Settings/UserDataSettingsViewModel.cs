using CommunityToolkit.Mvvm.ComponentModel;

using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Settings;

public partial class UserDataSettingsViewModel : ViewModelBase, ISettingsViewChild {

	[ObservableProperty]
	private string usernameTextboxText = "";

	[ObservableProperty]
	private string startDateTextboxText = "";

	[ObservableProperty]
	private string jobNameTextboxText = "";

	public UserDataSettingsViewModel(DateTimeService dateTimeService, SettingsService settingsService) : base() {
		settingsService.OnUsernameChanged +=
			val => UsernameTextboxText = settingsService.Username;
		settingsService.OnStartDateChanged +=
			val => StartDateTextboxText = settingsService.StartDateString;
		settingsService.OnJobNameChanged +=
			val => JobNameTextboxText = settingsService.JobName;
		settingsService.OnPreSettingsSave += () => {
			settingsService.Username = UsernameTextboxText;
			settingsService.StartDateString = StartDateTextboxText;
			settingsService.JobName = JobNameTextboxText;
		};
		UsernameTextboxText = settingsService.Username;
		StartDateTextboxText = settingsService.StartDateString;
		JobNameTextboxText = settingsService.JobName;
	}
}
