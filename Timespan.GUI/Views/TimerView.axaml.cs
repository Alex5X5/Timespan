namespace Timespan.GUI.Views;

using Timespan.Util.Attributes;
using Timespan.Util.Services;

internal partial class TimerView : UserControl {


    [TranslateMember("Views.Pages.Timer.Labels.Title", "Timer")]
    public string TitleLabelText { get; set; } = "";
	
    [TranslateMember("Views.Pages.Timer.Labels.Description", "Description")]
    public string DescriptionLabelText { get; set; } = "";

	[TranslateMember("Views.Pages.Timer.Labels.Start", "Start")]
    public string StartLabelText { get; set; } = "";

	[TranslateMember("Views.Pages.Timer.Labels.Stop", "Finish")]
    public string FinishLabelText { get; set; } = "";


	[TranslateMember("Views.Pages.Timer.Buttons.Start", "Start")]
    public string StartButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Timer.Buttons.Stop", "Stop")]
    public string StopButtonText { get; set; } = "";

	public TimerView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
        InitializeComponent();
	}
}