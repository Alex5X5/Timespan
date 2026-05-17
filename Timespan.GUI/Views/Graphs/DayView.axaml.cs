namespace Timespan.GUI.Views.Graphs;

using Avalonia.Controls;

using Timespan.Util.Attributes;
using Timespan.Util.Services;

public partial class DayView : UserControl {


	[TranslateMember("Views.Pages.Timer.Labels.Title", "Timer")]
	public string TitleLabelText { get; set; } = "";

	public DayView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
		InitializeComponent();
    }
}