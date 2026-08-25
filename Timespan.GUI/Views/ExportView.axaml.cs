namespace Timespan.GUI.Views;

using Avalonia.Input;

using Timespan.Util.Attributes;
using Timespan.Util.Services;

using Timespan.GUI.ViewModels;

internal partial class ExportView : UserControl {

	[TranslateMember("Views.Pages.Export.Buttons.Import", "Import")]
	public string ImportButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Export.Buttons.Export", "Export")]
	public string ExportButtonText { get; set; } = "";

	[TranslateMember("Views.Pages.Export.Buttons.Folder", "Open Folder")]
	public string FolderButtonText { get; set; } = "";

	public ExportView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
        InitializeComponent();
	}

	private void UserControlLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		Console.WriteLine("Export View loaded!");
		(DataContext as ExportViewModel)?.OnLoad();
	}

	private void TextBoxFocused(object sender, FocusChangedEventArgs e) {
		if (sender is TextBox textBox && textBox.DataContext is TextboxItem item)
			if (item.Task != null)
				(DataContext as ExportViewModel)?.OnTaskRedirect(item.Task);
	}
}