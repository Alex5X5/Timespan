namespace Hourglass.GUI.ViewModels.Pages;

using Hourglass.GUI.ViewModels.Components.GraphPanels;
using ReactiveUI;
using System.Collections.ObjectModel;
using static Hourglass.GUI.ViewModels.MainViewModel;

public class GraphPageViewModel : MainViewChildPageViewModel {

	private GraphPanelViewModelBase _CurrentGraphPanel;
	public GraphPanelViewModelBase CurrentGraphPanel {
		get => _CurrentGraphPanel;
		private set {
			Console.WriteLine($"settin current page to {value.GetType().Name}");
			this.RaiseAndSetIfChanged(ref _CurrentGraphPanel, value);
			controller.RaisePropertyChanged(nameof(controller.Title));
		}
	}

	private ViewModelFactory<GraphPanelViewModelBase> panelFactory;
	private MainViewModel controller;

	public override string Title => _CurrentGraphPanel?.Title ?? "";

	public GraphPageViewModel() : this(null, null) {

	}

	public GraphPageViewModel(ViewModelFactory<GraphPanelViewModelBase> panelFactory, MainViewModel controller) : base() {
		this.panelFactory = panelFactory;
		this.controller = controller;
        Console.WriteLine("constructing graph page view model");
    }

    public void ChangeGraphPanel<PanelT>() where PanelT : GraphPanelViewModelBase {
		CurrentGraphPanel = panelFactory.GetPageViewModel<PanelT>();
		Console.WriteLine($"chaged type of panel to:{_CurrentGraphPanel.GetType().Name}");
	}
}