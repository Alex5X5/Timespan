namespace Hourglass.GUI.Views.Pages;

using Avalonia.Controls;
using Avalonia.Input;

using Hourglass.GUI.ViewModels;
using Hourglass.GUI.ViewModels.Pages;
using Timespan.Util.Attributes;

public partial class TimerPageView : PageViewBase {


    [TranslateMember("Views.Pages.Timer.Labels.Title", "Timer")]
    public string TitleLabelText { get; set; }
	
    [TranslateMember("Views.Pages.Timer.Labels.Description", "Description")]
    public string DescriptionLabelText { get; set; }

    [TranslateMember("Views.Pages.Timer.Labels.Start", "Start")]
    public string StartLabelText { get; set; }

    [TranslateMember("Views.Pages.Timer.Labels.Stop", "Finish")]
    public string FinishLabelText { get; set; }


    [TranslateMember("Views.Pages.Timer.Buttons.Start", "Start")]
    public string StartButtonText { get; set; }

    [TranslateMember("Views.Pages.Timer.Buttons.Stop", "Stop")]
    public string StopButtonText { get; set; }


    public TimerPageView() : this(null) {
		
	}

	public TimerPageView(ViewModelBase? model) : base(model) {
		InitializeComponent();
		startButton.GotFocus += (sender, args) => {
			startButton.InvalidateVisual();
		};
	}

	private void TextBox_GotFocus(object? sender, Avalonia.Input.GotFocusEventArgs e) {
		//Console.WriteLine("got focus!");
		//if (initialDescriptionTextboxClear) {
		//	Console.WriteLine("initial focus!");
		//	DescriptionTextbox.Clear();
		//	initialDescriptionTextboxClear = false;
		//}
	}

	private void TextBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) {
		if (e.Key == Key.Escape)
			TopLevel.GetTopLevel(this)?.Focus();
		if (e.Key == Key.Enter) {
			startButton.Focus();
		}
	}

	private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		(DataContext as TimerPageViewModel)?.OnLoad();
		Console.WriteLine("Timer Page loaded!");
    }

    private void UserControl_Unloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        (DataContext as TimerPageViewModel)?.OnUnload();
        Console.WriteLine("Timer Page unloaded!");
    }
}