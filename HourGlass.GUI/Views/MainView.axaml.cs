namespace Hourglass.GUI.Views;

using Hourglass.GUI.ViewModels;
using Hourglass.GUI.ViewModels.Pages;
using Hourglass.Util.Attributes;

public partial class MainView : ViewBase {

    [TranslateMember("Views.MainView.Buttons.Timer", "Timer")]
    public string TimerModeButtonText { get; set; } = "";

    [TranslateMember("Views.MainView.Buttons.Graphs", "Graphs")]
    public string GraphModeButtonText { get; set; } = "";

    [TranslateMember("Views.MainView.Buttons.Export", "Export")]
    public string ExportModeButtonText { get; set; } = "";


	public MainView() : base() {
		InitializeComponent();
		if (DataContext is MainViewModel viewModel) {
			viewModel.ChangePage<TimerPageViewModel>();
		}
	}

	private void TimerModeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		if (DataContext is MainViewModel viewModel)
			viewModel.GoToTimer();
	}

	private void GraphModeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		if (DataContext is MainViewModel viewModel)
			viewModel.GoToGraphs();
	}

	private void ExportModeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		if (DataContext is MainViewModel viewModel)
			viewModel.GoToExport();
	}


    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        Console.WriteLine("Settings Page loaded!");
        (DataContext as MainViewModel)?.OnLoad();
    }
}
