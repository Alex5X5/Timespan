namespace Timespan.GUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;
using Timespan.GUI.ViewModels.Graphs;
using Timespan.Util.Services;

public partial class GraphsViewModel : ViewModelBase, IMainViewChild {

	private IHourglassDbService dbService;

	public RedirectionAnchor<IGraphsViewChild> CurrentPageAnchor;
	public IGraphsViewChild? CurrentPage => CurrentPageAnchor.CurrentModel;

	private string selectedTimeMode = "";
	public string SelectedTimeMode {
		set {
			selectedTimeMode = value;
			UpdateMode(value);
			OnPropertyChanged(nameof(SelectedTimeMode));
			OnPropertyChanged(nameof(CurrentPage));
			OnPropertyChanged(nameof(DateString));
		}
		get => selectedTimeMode;
	}

	public ObservableCollection<string> TimeModes { get; }
	
	public string DateString => CurrentPage?.GetDateString() ?? "Date";

	[ObservableProperty]
	private bool showTaskPanel = false;

    [ObservableProperty]
    private string showingTaskTitle = "a title";

    [ObservableProperty]
    private string showingTaskDescription = "lorem ipsum dolor sit amet condecteter";

    [ObservableProperty]
    private string showingTaskDateString = "Mi. 18. Feb. 2026";

    [ObservableProperty]
    private string showingTaskTimeString = "07:34 - 11:53";

    partial void OnShowTaskPanelChanged(bool value)
	{
		SpacerWidth = new(value ? 1 : 0, GridUnitType.Star);
		TaskPanelWidth = new(value ? 19 : 0, GridUnitType.Star);
	}

	[ObservableProperty]
	private GridLength spacerWidth = new(0, GridUnitType.Star);

	[ObservableProperty]
	private GridLength taskPanelWidth = new(0, GridUnitType.Star);

	public GraphsViewModel(RedirectionService redirectionService, ViewModelFactory<IGraphsViewChild> factory, IHourglassDbService dbService) : base() {
		this.dbService = dbService;
		CurrentPageAnchor = new(factory);
		redirectionService.Register<GraphsViewModel, IGraphsViewChild>(CurrentPageAnchor);
		CurrentPageAnchor.ModelChanged += (from, to) => {
			OnPropertyChanged(nameof(CurrentPage));
			OnPropertyChanged(nameof(DateString));
		};
		TimeModes = ["Day", "Week", "Month"];
		SelectedTimeMode = TimeModes[0];
		if(GlobalEventService.GetEvent<ShowTaksEventArgs>() is EventDispatcher<ShowTaksEventArgs> dispatcher)
			dispatcher += ShowTask;
		CurrentPageAnchor.ChangeModel<DayViewModel>();
	}

	[RelayCommand]
	internal void ShowTask(ShowTaksEventArgs args) {
		Console.WriteLine("[GraphsView]:showing task");
		if (args.Task is null) {
			HideTask();
		} else {
			ShowTaskPanel = true;
			ShowingTaskDescription = args.Task.description;
			string day = TranslatorService.Singleton.TranslateDay(args.Task.StartDateTime.DayOfWeek);
			string month = TranslatorService.Singleton.TranslateMonth(args.Task.StartDateTime.Month);
			ShowingTaskDateString = $"{day}. {args.Task.StartDateTime.Day}. {month}. {args.Task.StartDateTime.Year}";
			
		}
	}

	[RelayCommand]
	internal void HideTask() {
		Console.WriteLine("[GraphsView]:hiding task");
		ShowTaskPanel = false;
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
		if(mode == TimeModes[0])
			CurrentPageAnchor.ChangeModel<DayViewModel>();
		if (mode == TimeModes[1])
			CurrentPageAnchor.ChangeModel<WeekViewModel>();
		if (mode == TimeModes[2])
			CurrentPageAnchor.ChangeModel<MonthViewModel>();
	}

	internal void OnLoad() {
	}
}
