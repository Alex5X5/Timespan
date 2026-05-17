using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Hourglass.GUI.ViewModels.Pages;
using Timespan.Util.Attributes;

namespace Hourglass.GUI.Views.Pages;

public partial class ExportPageView : PageViewBase {

    [TranslateMember("Views.Pages.Export.Buttons.Import", "Import")]
    public string ImportButtonText { get; set; } = "";

    [TranslateMember("Views.Pages.Export.Buttons.Export", "Export")]
    public string ExportButtonText { get; set; } = "";

    [TranslateMember("Views.Pages.Export.Buttons.Folder", "Open Folder")]
    public string FolderButtonText { get; set; } = "";

    public ExportPageView() : base() {
		InitializeComponent();
    }
    
    private void TextBox_Focused(object sender, GotFocusEventArgs e) {
        if (sender is TextBox textBox && textBox.DataContext is TextboxItem item)
            if(item.Task!=null)
                (DataContext as ExportPageViewModel)?.OnTaskRedirect(item.Task);
    }
}

    