namespace Hourglass.GUI.Views.Pages.SettingsPages;

using Hourglass.GUI.ViewModels.Pages.SettingsPages;
using Timespan.Util.Attributes;

public partial class GeneralSubSettingsPageView: SubSettingsPageViewBase {

    [TranslateMember("Views.Pages.Settings.General.Labels.Language", "Language")]
    public string LanguagesLabelText { get; set; } = "";

    public GeneralSubSettingsPageView() : base() {
		InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        (DataContext as GeneralSubSettingsPageViewModel)?.OnLoad();
    }
}