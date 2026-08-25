namespace Timespan.GUI.Views.Settings;

using Timespan.Util.Attributes;
using Timespan.Util.Services;

public partial class GeneralSettingsView : UserControl {

	[TranslateMember("Views.Pages.Settings.General.Labels.Title", "General")]
	public string TitleLabelText { get; set; } = "";


	[TranslateMember("Views.Pages.Settings.General.Labels.Language", "Language")]
	public string LanguageLabelText { get; set; } = "";

	public GeneralSettingsView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
		InitializeComponent();
    }
}