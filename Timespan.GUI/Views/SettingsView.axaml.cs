using Timespan.GUI.ViewModels;

namespace Timespan.GUI.Views;

public partial class SettingsView : UserControl {

	public SettingsView() {
        InitializeComponent();
	}

	private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		Console.WriteLine("Main View loaded!");
		(DataContext as SettingsViewModel)?.OnLoad();
	}
}