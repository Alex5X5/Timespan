using Timespan.Util.Attributes;
using Timespan.Util.Services;

namespace Timespan.GUI.Views.Settings;

public partial class UserDataSettingsView : UserControl {


	[TranslateMember("Views.Pages.Settings.UserData.Labels.Title", "User Data")]
	public string TitleLabelText { get; set; } = "";

	[TranslateMember("Views.Pages.Settings.UserData.Labels.Username", "Username")]
	public string UsernamePlaceholderText { get; set; } = "";

	[TranslateMember("Views.Pages.Settings.UserData.Labels.StartDate", "Start Date")]
	public string StartDatePlaceholderText { get; set; } = "";

	[TranslateMember("Views.Pages.Settings.UserData.Labels.JobName", "Job Name")]
	public string JobNamePlaceholderText { get; set; } = "";

	public UserDataSettingsView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
		InitializeComponent();
    }
}