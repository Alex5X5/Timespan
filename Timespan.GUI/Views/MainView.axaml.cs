using Timespan.GUI.ViewModels;
using Timespan.Util.Attributes;
using Timespan.Util.Services;

namespace Timespan.GUI.Views;

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
	}

	private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		Console.WriteLine("Main View loaded!");
		(DataContext as MainViewModel)?.OnLoad();
	}
}