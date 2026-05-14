namespace Timespan.GUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.ObjectModel;

using Timespan.GUI.Services;
using Timespan.GUI.ViewModels.Graphs;

public partial class GraphsViewModel : ViewModelBase, IMainViewChild {

	public RedirectionAnchor<IGraphsViewChild> CurrentPageAnchor;
	public IGraphsViewChild? CurrentPage => CurrentPageAnchor.CurrentModel;

	private string selectedItem = "";
	public string SelectedItem {
		set {
			selectedItem = value;
			UpdateMode(value);
			OnPropertyChanged(nameof(SelectedItem));
			OnPropertyChanged(nameof(DateString));
		}
		get => selectedItem;
	}

	public ObservableCollection<string> Items { get; }
	
	public string DateString => CurrentPage?.GetDateString() ?? "";

	public GraphsViewModel(RedirectionService redirectionService, ViewModelFactory<IGraphsViewChild> factory) : base() {
		CurrentPageAnchor = new(factory);
		redirectionService.Register<GraphsViewModel, IGraphsViewChild>(CurrentPageAnchor);
		CurrentPageAnchor.ModelChanged += () => {
			OnPropertyChanged(nameof(CurrentPage));
			OnPropertyChanged(nameof(DateString));
		};
		Items = new() { "Day", "Week", "Month" };
		SelectedItem = Items[0];
	}

	private void UpdateMode(string mode) {
		if(mode == Items[0])
			CurrentPageAnchor.ChangeModel<DayViewModel>();
		if (mode == Items[1])
			CurrentPageAnchor.ChangeModel<WeekViewModel>();
		if (mode == Items[2])
			CurrentPageAnchor.ChangeModel<MonthViewModel>();
	}

	internal void OnLoad() {
		CurrentPageAnchor.ChangeModel<DayViewModel>();
	}
}
