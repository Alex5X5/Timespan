namespace Hourglass.GUI.Views.Pages;

using Hourglass.GUI.ViewModels;
using Hourglass.GUI.ViewModels.Components.GraphPanels;
using Hourglass.GUI.ViewModels.Pages;

public partial class GraphPageView : PageViewBase {

	public GraphPageView() : this(null) {
		
	}

	public GraphPageView(GraphPageViewModel? model) : base(model) {
		InitializeComponent();
	}

	private void DayModeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		Console.WriteLine("day mode button click!");
		if (DataContext is GraphPageViewModel viewModel)
			viewModel.ChangeGraphPanel<DayGraphPanelViewModel>();
	}

	private void WeekModeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		Console.WriteLine("week mode button click!");
		if (DataContext is GraphPageViewModel viewModel)
			viewModel.ChangeGraphPanel<WeekGraphPanelViewModel>();
	}

	private void MonthModeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		Console.WriteLine("month mode button click!");
		if (DataContext is GraphPageViewModel viewModel)
			viewModel.ChangeGraphPanel<MonthGraphPanelViewModel>();
	}
}