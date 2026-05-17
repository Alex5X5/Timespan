namespace Hourglass.GUI.Views.Pages.SettingsPages;

using Hourglass.GUI.ViewModels.Pages.SettingsPages;
using Timespan.Util.Attributes;

public partial class VisualsSubSettingsPageView : SubSettingsPageViewBase {

    [TranslateMember("Views.Pages.Settings.Visuals.Labels.Themes", "Themes")]
    public string ThemesLabelText { get; set; } = "";

    public VisualsSubSettingsPageView() : base() {
		InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        (DataContext as VisualsSubSettingsPageViewModel)?.OnLoad();
    }
}