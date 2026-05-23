using Timespan.Util.Attributes;
using Timespan.Util.Services;

namespace Timespan.GUI.Views.Settings;

public partial class ExportSettingsView : UserControl {

	[TranslateMember("Views.Pages.Settings.Export.Labels.Title", "Export")]
	public string TitleLabelText { get; set; } = "";

	public ExportSettingsView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
		InitializeComponent();
    }
}