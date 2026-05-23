using Timespan.GUI.ViewModels;
using Timespan.Util.Attributes;
using Timespan.Util.Services;

namespace Timespan.GUI.Views;

internal partial class MainView : UserControl {

	[TranslateMember("Views.Pages.Timer.Labels.Title", "Timer")]
	public string TimerButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Graphs.Labels.Title", "Graphs")]
	public string GraphsButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Export.Labels.Title", "Export")]
	public string ExportButtonText { get; set; } = "";


	[TranslateMember("Views.Pages.Settings.About.Labels.Title", "About")]
	public string AboutSettingsButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Settings.Export.Labels.Title", "Export")]
	public string ExportSettingsButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Settings.General.Labels.Title", "Username")]
	public string GeneralSettingsButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Settings.Graphics.Labels.Title", "Graphics")]
	public string GraphicsSettingsButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Settings.UserData.Labels.Title", "User Data")]
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