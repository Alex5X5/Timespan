namespace Timespan.GUI.Views;

using Avalonia.Interactivity;

using Timespan.GUI.ViewModels;
using Timespan.Util.Attributes;
using Timespan.Util.Services;

internal partial class MainView : UserControl {

	[TranslateMember("Views.Pages.Main.Buttons.Timer", "Timer")]
	public string TimerButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Main.Buttons.Graphs", "Graphs")]
	public string GraphsButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Main.Buttons.Export", "Export")]
	public string ExportButtonText { get; set; } = "";


	[TranslateMember("Views.Pages.Main.Buttons.About", "About")]
	public string AboutSettingsButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Main.Buttons.Export", "Export")]
	public string ExportSettingsButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Main.Buttons.General", "General")]
	public string GeneralSettingsButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Main.Buttons.Graphics", "Graphics")]
	public string GraphicsSettingsButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Main.Buttons.UserData", "User Data")]
	public string UserDataSettingsButtonText { get; set; } = "";

	public MainView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
		InitializeComponent();
		AddHandler(LoadedEvent, OnLoad);
		AddHandler(UnloadedEvent, OnUnload);
	}

	private void OnLoad(object? sender, RoutedEventArgs args) {
		Console.WriteLine("Main View loaded!");
		(DataContext as MainViewModel)?.OnLoad();
	}

	private void OnUnload(object? sender, RoutedEventArgs args) {
		Console.WriteLine("Main View loaded!");
		(DataContext as MainViewModel)?.OnUnload();
	}
}