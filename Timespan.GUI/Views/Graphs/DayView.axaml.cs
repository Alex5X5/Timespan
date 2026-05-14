namespace Timespan.GUI.Views.Graphs;

using Avalonia.Controls;

using Hourglass.Util.Attributes;
using Hourglass.Util.Services;

public partial class DayView : UserControl {


	[TranslateMember("Views.Pages.Timer.Labels.Title", "Timer")]
	public string TitleLabelText { get; set; } = "";

	public DayView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
		InitializeComponent();
    }
}