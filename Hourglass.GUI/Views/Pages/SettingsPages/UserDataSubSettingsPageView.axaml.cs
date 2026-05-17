namespace Hourglass.GUI.Views.Pages.SettingsPages;

using Avalonia.Controls;

using Hourglass.GUI.ViewModels.Pages.SettingsPages;
using Timespan.Util.Attributes;

public partial class UserDataSubSettingsPageView : SubSettingsPageViewBase {

    [TranslateMember("Views.Pages.Settings.UserData.Labels.Username", "Username")]
    public string UsernameLabelText { get; set; } = "";

    [TranslateMember("Views.Pages.Settings.UserData.Labels.StartDate", "Start Date")]
    public string StartDateLabelText { get; set; } = "";

    [TranslateMember("Views.Pages.Settings.UserData.Labels.JobName", "Job Name")]
    public string JobNameLabelText { get; set; } = "";


    public UserDataSubSettingsPageView() : base() {
		InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        (DataContext as UserDataSubSettingsPageViewModel)?.OnLoad();
    }

    private void TextBox_SizeChanged(object sender, SizeChangedEventArgs e) {
        
    }

}