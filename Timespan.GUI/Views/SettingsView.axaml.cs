using Timespan.Util.Attributes;
using Timespan.Util.Services;

using Timespan.GUI.ViewModels;

namespace Timespan.GUI.Views;

public partial class SettingsView : UserControl {
	
	[TranslateMember("Views.Pages.Settings.Buttons.Save", "Save")]
	public string SaveButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Settings.Buttons.Cancel", "Cancel")]
	public string CancelButtonText { get; set; } = "";

	public SettingsView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
		InitializeComponent();
	}

	private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		Console.WriteLine("Settings View loaded!");
		(DataContext as SettingsViewModel)?.OnLoad();
	}
}