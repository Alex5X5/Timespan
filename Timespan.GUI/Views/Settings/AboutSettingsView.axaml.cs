using Timespan.GUI.ViewModels.Settings;
using Timespan.Util.Attributes;

namespace Timespan.GUI.Views.Settings;

public partial class AboutSettingsView : UserControl {

	[TranslateMember("Views.Pages.Settings.About.Labels.Title", "About")]
	public string TitleLabelText { get; set; } = "";

	public AboutSettingsView()
    {
        InitializeComponent();
	}

	private void UserControl_Unloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		//(DataContext as AboutSettingsViewModel)?.OnUnload();
		Console.WriteLine("Timer Page unloaded!");
	}
}