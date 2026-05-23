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
		settingsService.OnPreSettingsSave += OnPreSettingsSave;
		UsernameTextboxText = settingsService.Username;
		StartDateTextboxText = settingsService.StartDateString;
		JobNameTextboxText = settingsService.JobName;
	}

	private void OnPreSettingsSave() {
		settingsService.Username = UsernameTextboxText;
		settingsService.StartDateString = StartDateTextboxText;
		settingsService.JobName = JobNameTextboxText;
	}

	public void OnUnload() {
		settingsService.OnPreSettingsSave -= OnPreSettingsSave;
	}
}
