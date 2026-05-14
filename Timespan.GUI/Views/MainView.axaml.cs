using Timespan.GUI.ViewModels;

namespace Timespan.GUI.Views;

internal partial class MainView : UserControl {

    public MainView()
    {
        InitializeComponent();
	}

	private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		Console.WriteLine("Main View loaded!");
		(DataContext as MainViewModel)?.OnLoad();
	}
}