using Hourglass.Util.Attributes;
using Hourglass.Util.Services;

using Timespan.GUI.ViewModels;

namespace Timespan.GUI.Views;

internal partial class GraphsView : UserControl {

	[TranslateMember("Views.Pages.Graphs.Labels.Title", "Graphs")]
	public string TitleLabelText { get; set; } = "";
	[TranslateMember("Views.Pages.Graphs.Buttons.Delete", "Delete")]
	public string DeleteButtonText { get; set; } = "";
	[TranslateMember("Views.Pages.Graphs.Buttons.Select", "Select")]
	public string SelectButtonText { get; set; } = "";


	public GraphsView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
        InitializeComponent();
	}

	private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		Console.WriteLine("Graphs View loaded!");
		(DataContext as GraphsViewModel)?.OnLoad();
	}
}