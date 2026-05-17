namespace Hourglass.GUI.Views.Pages;

using Hourglass.GUI.ViewModels.Pages;
using Timespan.Util.Attributes;

public partial class SettingsPageView : PageViewBase {

    [TranslateMember("Views.Pages.Settings.Buttons.Back", "Back")]
    public string BackButtonText { get; set; } = "";

    [TranslateMember("Views.Pages.Settings.Buttons.General", "General")]
    public string GeneralButtonText { get; set; } = "";

    [TranslateMember("Views.Pages.Settings.Buttons.UserData", "User Data")]
    public string UserDataButtonText { get; set; } = "";

    [TranslateMember("Views.Pages.Settings.Buttons.About", "About")]
    public string AboutButtonText { get; set; } = "";

    [TranslateMember("Views.Pages.Settings.Buttons.Visual", "Visual")]
    public string VisualButtonText { get; set; } = "";

    [TranslateMember("Views.Pages.Settings.Buttons.Export", "Export")]
    public string ExportButtonText { get; set; } = "";

    [TranslateMember("Views.Pages.Settings.Buttons.Save", "Save")]
    public string SaveButtonText { get; set; } = "";

    [TranslateMember("Views.Pages.Settings.Buttons.Cancel", "Cancel")]
    public string CancelButtonText { get; set; } = "";


    public SettingsPageView() : base() {
		InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        Console.WriteLine("Settings Page loaded!");
        (DataContext as SettingsPageViewModel)?.OnLoad();
    }
}
