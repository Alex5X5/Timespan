namespace Timespan.GUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types;
using Timespan.GUI.ViewModels.Graphs;

public partial class GraphsViewModel : ViewModelBase, IMainViewChild {

	private IHourglassDbService dbService;

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
	
	public string DateString => CurrentPage?.GetDateString() ?? "Date";

	[ObservableProperty]
	private GridLength spacerWidth = new(0, GridUnitType.Star);

	[ObservableProperty]
	private GridLength taskPanelWidth = new(0, GridUnitType.Star);

	public GraphsViewModel(RedirectionService redirectionService, ViewModelFactory<IGraphsViewChild> factory, IHourglassDbService dbService) : base() {
		this.dbService = dbService;
		CurrentPageAnchor = new(factory);
		redirectionService.Register<GraphsViewModel, IGraphsViewChild>(CurrentPageAnchor);
		CurrentPageAnchor.ModelChanged += () => {
			OnPropertyChanged(nameof(CurrentPage));
			OnPropertyChanged(nameof(DateString));
		};
		Items = new() { "Day", "Week", "Month" };
		SelectedItem = Items[0];
		if(GlobalEventService.GetEvent<ShowTaksEventArgs>() is EventDispatcher<ShowTaksEventArgs> dispatcher)
			dispatcher += ShowTask;
	}

	[RelayCommand]
	internal void ShowTask(ShowTaksEventArgs args) {
		Console.WriteLine("[GraphsView]:showing task");
		if (args.Task is null) {
			SpacerWidth = new GridLength(0, GridUnitType.Star);
			TaskPanelWidth = new GridLength(0, GridUnitType.Star);
		} else {
			SpacerWidth = new GridLength(2, GridUnitType.Star);
			TaskPanelWidth = new GridLength(24, GridUnitType.Star);

		}
	}

	[RelayCommand]
	internal void HideTask() {
		Console.WriteLine("[GraphsView]:hiding task");
		SpacerWidth = new GridLength(0, GridUnitType.Star);
		TaskPanelWidth = new GridLength(0, GridUnitType.Star);
	}

	[RelayCommand]
	internal void DeleteTask() {
		HideTask();
	}

	[RelayCommand]
	internal void EditTask() {
		
	}

	[RelayCommand]
	internal void Select() {
		if (dbService.QueryCurrentTaskAsync().Result is Timespan.Types.Models.Task task) {
			var args = new ShowTaksEventArgs(task);
			GlobalEventService.Raise(args);
		}
	}

	[RelayCommand]
	internal void Delete() {
		var args = new ShowTaksEventArgs();
		GlobalEventService.Raise(args);
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
