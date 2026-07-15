namespace Timespan.GUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Services.Mapping;
using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;
using Timespan.GUI.ViewModels.Graphs;
using Timespan.Util.Services;

public partial class GraphsViewModel : ViewModelBase, IMainViewChild {

	private ITimespanDbService dbService;
	private readonly GuiStateService stateService;

	public IRedirectionAnchor<IGraphsViewChild> CurrentPageAnchor;
	public IGraphsViewChild? CurrentPage => CurrentPageAnchor.CurrentModel;

	private string selectedTimeMode = "";
	public string SelectedTimeMode {
		set {
			selectedTimeMode = value;
			UpdateMode(value);
		}
		get => selectedTimeMode;
	}

	public ObservableCollection<string> TimeModes { get; }
	
	[ObservableProperty]
	private string dateString = "";

	[ObservableProperty]
	private bool showTaskPanel = false;

	[ObservableProperty]
	private ObservableTask showingTask;

    partial void OnShowTaskPanelChanged(bool value)
	{
		SpacerWidth = new(value ? 1 : 0, GridUnitType.Star);
		TaskPanelWidth = new(value ? 19 : 0, GridUnitType.Star);
		HeaderSpacerWidth = new(value ? 15 : 5, GridUnitType.Star);
	}

	[ObservableProperty]
	private GridLength spacerWidth = new(0, GridUnitType.Star);

	[ObservableProperty]
	private GridLength headerSpacerWidth = new(0, GridUnitType.Star);

	[ObservableProperty]
	private GridLength taskPanelWidth = new(0, GridUnitType.Star);

	public GraphsViewModel(RedirectionService redirectionService, ITimespanDbService dbService, GuiStateService stateService) : base() {
		this.dbService = dbService;
		this.stateService = stateService;
		CurrentPageAnchor = new RedirectionAnchor<IGraphsViewChild>();
		redirectionService.Register<GraphsViewModel, IGraphsViewChild>(CurrentPageAnchor);
		CurrentPageAnchor.ModelChanged += (from, to) => {
			OnPropertyChanged(nameof(CurrentPage));
			OnPropertyChanged(nameof(DateString));
		};
		TimeModes = [
			TranslatorService.Singleton["TimeMode.Day"],
			TranslatorService.Singleton["TimeMode.Week"],
			TranslatorService.Singleton["TimeMode.Month"],
		];
		SelectedTimeMode = TimeModes[0];
		GlobalEventService.Subscribe<ShowTaksEventArgs>(ShowTask);
		stateService.SelectedDay = DateTimeService.FloorDay(DateTime.Now);
		GlobalEventService.Raise<IntervallChangedEventArgs>();
		CurrentPageAnchor.ChangeModel<DayPanelViewModel>();
	}

	[RelayCommand]
	internal void ShowTask(ShowTaksEventArgs args) {
		if (args.Task is null) {
			HideTask();
		} else {
			ShowingTask = TaskMapper.ToGuiType(args.Task);
			ShowTaskPanel = true;
		}
	}

	[RelayCommand]
	internal void HideTask() {
		ShowTaskPanel = false;
	}

	[RelayCommand]
	internal async Task Select() {
		var task = await dbService.QueryCurrentTaskAsync();
		if (task is Timespan.Types.Models.Task) {
			var args = new ShowTaksEventArgs(task);
			GlobalEventService.Raise(args);
		}
	}

	[RelayCommand]
	internal void Delete() {
	}

	[RelayCommand]
	protected void PreviousIntervallClick() {
		if (SelectedTimeMode == TimeModes[0])
			stateService.SelectedDay = DateTimeService.FloorDay(stateService.SelectedDay.AddDays(-1));
		if (SelectedTimeMode == TimeModes[1])
			stateService.SelectedDay = DateTimeService.FloorWeek(stateService.SelectedDay.AddDays(-7));
		if (SelectedTimeMode == TimeModes[2])
			stateService.SelectedDay = DateTimeService.FloorMonth(stateService.SelectedDay.AddMonths(-1));
		DateString = CurrentPage?.GetDateString() ?? "Date";
		GlobalEventService.Raise<IntervallChangedEventArgs>();
		GlobalEventService.Raise<TasksChangedEventArgs>();
	}

	[RelayCommand]
	protected void FollowingIntervallClick() {
		if (SelectedTimeMode == TimeModes[0])
			stateService.SelectedDay = DateTimeService.FloorDay(stateService.SelectedDay.AddDays(1));
		if (SelectedTimeMode == TimeModes[1])
			stateService.SelectedDay = DateTimeService.FloorWeek(stateService.SelectedDay.AddDays(7));
		if (SelectedTimeMode == TimeModes[2])
			stateService.SelectedDay = DateTimeService.FloorMonth(stateService.SelectedDay.AddMonths(1));
		DateString = CurrentPage?.GetDateString() ?? "Date";
		GlobalEventService.Raise<IntervallChangedEventArgs>();
		GlobalEventService.Raise<TasksChangedEventArgs>();
	}

	private void UpdateMode(string mode) {
		if(mode == TimeModes[0])
			CurrentPageAnchor.ChangeModel<DayPanelViewModel>();
		if (mode == TimeModes[1])
			CurrentPageAnchor.ChangeModel<WeekPanelViewModel>();
		if (mode == TimeModes[2])
			CurrentPageAnchor.ChangeModel<MonthPanelViewModel>();
		DateString = CurrentPage?.GetDateString() ?? "Date";
		OnPropertyChanged(nameof(SelectedTimeMode));
		OnPropertyChanged(nameof(CurrentPage));
		OnPropertyChanged(nameof(DateString));
	}

	[RelayCommand]
	internal void OnLoad() {
		GlobalEventService.Subscribe<IntervallChangedEventArgs>(UpdateIntervall);
	}

	[RelayCommand]
	internal void OnUnLoad() {
		HideTask();
		GlobalEventService.UnSubscribe<IntervallChangedEventArgs>(UpdateIntervall);
	}

	private void UpdateIntervall(IntervallChangedEventArgs args) {
		DateString = CurrentPage?.GetDateString() ?? "Date";
	}
}
