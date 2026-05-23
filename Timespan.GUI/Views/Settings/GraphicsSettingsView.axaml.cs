using Timespan.Util.Attributes;
using Timespan.Util.Services;

namespace Timespan.GUI.Views.Settings;

public partial class GraphicsSettingsView : UserControl {


	[TranslateMember("Views.Pages.Settings.Graphics.Labels.Title", "Graphics")]
	public string TitleLabelText { get; set; } = "";


	[TranslateMember("Views.Pages.Settings.Graphics.Labels.Theme", "Theme")]
	public string ThemeLabelText { get; set; } = "";

	public GraphicsSettingsView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
		InitializeComponent();
    }
}