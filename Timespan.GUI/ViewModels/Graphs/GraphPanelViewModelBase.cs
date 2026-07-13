namespace Timespan.GUI.ViewModels.Graphs;

using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Helpers;
using Timespan.GUI.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Services.Mapping;
using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

public abstract partial class GraphPanelViewModelBase : ViewModelBase, IGraphsViewChild, IGraphViewModel {

	protected Services.CacheService cacheService;
	protected ITimespanDbService dbService;
	protected SettingsService settingsService;

	#region observable properties

	//[ObservableProperty]
	//public partial ObservableCollection<ObservableBool> MarkedRows { set; get; }

	//[ObservableProperty]
	//public partial ObservableCollection<ObservableBool> BlockedRows { set; get; }

	//[ObservableProperty]
	//public partial ObservableCollection<ObservableBool> MarkedColumns { set; get; }

	//[ObservableProperty]
	//public partial ObservableCollection<ObservableBool> BlockedColumns { set; get; }

	[ObservableProperty]
	public partial ObservableCollection<ObservableTask> Tasks { set; get; }

	[ObservableProperty]
	private bool[,] isTodaySegment = new bool[0, 0];

	[ObservableProperty]
	private ObservableBool[,] isBlocked;

	[ObservableProperty]
	private ObservableBool[,] isMarked;

	[ObservableProperty]
	private double extraClickSize = 1;
	
	[ObservableProperty]
	private double minimalWidth = 1;

	[ObservableProperty]
	private double maxTasks = 1;

	[ObservableProperty]
	private long xAxisSegmentDuration = 1;

	[ObservableProperty]
	private long xAxisSegmentCount = 0;

	[ObservableProperty]
	private long yAxisSegmentCount = 0;

	[ObservableProperty]
	private long taskGridRowCount = 0;

	[ObservableProperty]
	private long taskGridColumnCount = 0;

	[ObservableProperty]
	private long timeIntervallStartSeconds = 0;

	[ObservableProperty]
	private long timeIntervallStopSeconds = 1;

	[ObservableProperty]
	private DateTime selectedDay;

	[ObservableProperty]
	private bool suspendRendering;

	#endregion

	public GraphPanelViewModelBase(Services.CacheService cacheService, ITimespanDbService dbService, SettingsService settingsService, long start, long finish, int rows=1, int columns=24, int taskRows=1, int taskColumns=1, long duration=3600) : base() {
		this.cacheService = cacheService;
		this.dbService = dbService;
		this.settingsService = settingsService;
		suspendRendering = true;
		TimeIntervallStartSeconds = start;
		TimeIntervallStopSeconds = finish;
		XAxisSegmentDuration = duration;
		XAxisSegmentCount = columns;
		YAxisSegmentCount = rows;
		TaskGridRowCount = taskRows;
		TaskGridColumnCount = taskColumns;
		IsTodaySegment = new bool[YAxisSegmentCount, XAxisSegmentCount];
		IsBlocked = new ObservableBool[YAxisSegmentCount, XAxisSegmentCount];
		IsMarked = new ObservableBool[YAxisSegmentCount, XAxisSegmentCount];
		IsTodaySegment = new bool[YAxisSegmentCount, XAxisSegmentCount];
		for (int row = 0; row < YAxisSegmentCount; row++)
			for (int column = 0; column < XAxisSegmentCount; column++) {
				IsTodaySegment[row, column] = IsToday(row, column);
				IsBlocked[row, column] = new(false);
				IsMarked[row, column] = new(false);
			}
		suspendRendering = false;
		Tasks = new();
	}

	public abstract string GetDateString();

	protected abstract bool IsToday(int ro, int column);

	public virtual async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return dbService != null ? await dbService.QueryTasksOfDayAtDateAsync(DateTimeService.FloorDay(cacheService.SelectedDay)) : [];
	}

	#region commands

	[RelayCommand]
	protected virtual void OnLoad() {
		GlobalEventService.Subscribe<IntervallChangedEventArgs>(OnIntervallChanged);
		GlobalEventService.Subscribe<TasksChangedEventArgs>(OnTasksChanged);
		GlobalEventService.Subscribe<ShowTaksEventArgs>(OnShowTask);
		GlobalEventService.Raise<IntervallChangedEventArgs>();
		GlobalEventService.Raise<TasksChangedEventArgs>();
		UpdateColumnMarkers();
	}

	[RelayCommand]
	protected virtual void OnUnload() {
		GlobalEventService.UnSubscribe<IntervallChangedEventArgs>(OnIntervallChanged);
		GlobalEventService.UnSubscribe<TasksChangedEventArgs>(OnTasksChanged);
		GlobalEventService.UnSubscribe<ShowTaksEventArgs>(OnShowTask);
	}

	[RelayCommand]
	protected virtual void OnClicked() {
		
	}

	[RelayCommand]
	protected virtual void OnTaskClicked(TaskClickedEventArgs args) {
		cacheService.SelectedTask = args.Task;
		GlobalEventService.Raise(new ShowTaksEventArgs(args.Task));
	}

	[RelayCommand]
	protected async Task OnMissingContextMenuClicked(MissingContextClickedEventArgs args) {
		await SetTimeIntervallBlocked(args.Reason);
	}

	[RelayCommand]
	protected virtual void OnMousePressed(MousePressedEventArgs args) {
		if (!args.Right)
			for (int row = 0; row < YAxisSegmentCount; row++)
				for (int column = 0; column < XAxisSegmentCount; column++)
					IsMarked[row, column].Value = false;
	}

	[RelayCommand]
	protected virtual void OnMouseReleased(MouseReleasedEventArgs args) {
		
	}

	[RelayCommand]
	protected virtual void OnMouseDragging(MouseDraggingEventArgs args) {
	}

	#endregion

	#region property changed events

	protected virtual void OnIntervallChanged(IntervallChangedEventArgs args) {
		cacheService.SelectedDay = FloorIntervall(cacheService.SelectedDay);
		SelectedDay = cacheService.SelectedDay;
		TimeIntervallStartSeconds = DateTimeService.ToSeconds(FloorIntervall(cacheService.SelectedDay));
		TimeIntervallStopSeconds = DateTimeService.ToSeconds(CeilIntervall(cacheService.SelectedDay));
		UpdateColumnMarkers();
	}

	private async void OnTasksChanged(TasksChangedEventArgs args) {
		List<ObservableTask> tasks = (await GetTasksAsync())
			.Select(TaskMapper.ToGuiType)
			.ToList();
		await Task.Run(() => {
			if (tasks.Count == 0)
				Tasks = [];
			else
				Tasks = new(tasks);
		});
	}

	private void OnShowTask(ShowTaksEventArgs args) {
		 
	}

	protected abstract DateTime FloorIntervall(DateTime date);
	protected abstract DateTime CeilIntervall(DateTime date);

	partial void OnXAxisSegmentCountChanged(long value) {
		SuspendRendering = true;
		IsMarked = ArrayHelper.ResizeArray(IsMarked, (int)YAxisSegmentCount, (int)XAxisSegmentCount, (r, c) => new(false));
		IsBlocked = ArrayHelper.ResizeArray(IsBlocked, (int)YAxisSegmentCount, (int)XAxisSegmentCount, (r, c) => new(false));
		SuspendRendering = false;
	}

	partial void OnYAxisSegmentCountChanged(long value) {
		SuspendRendering = true;
		IsMarked = ArrayHelper.ResizeArray(IsMarked, (int)YAxisSegmentCount, (int)XAxisSegmentCount, (r, c) => new(false));
		IsBlocked = ArrayHelper.ResizeArray(IsBlocked, (int)YAxisSegmentCount, (int)XAxisSegmentCount, (r, c) => new(false));
		SuspendRendering = false;
	}

	#endregion

	#region marked rows and columns

	protected virtual void UpdateColumnMarkers() {
		List<Timespan.Types.Models.Task> tasks = dbService.QueryBlockingTasksInIntervallAsync(TimeIntervallStartSeconds, TimeIntervallStopSeconds).Result;
		ForeachCell(tasks, UpdateCellMarked);
	}

	private void UpdateCellMarked(int row, int column, long start, long finish, List<Timespan.Types.Models.Task> tasks) {
		IsBlocked[row, column].Value = tasks
			.Where(x => x.start >= start && x.start <= finish)
				.FirstOrDefault(x => x.finish >= start && x.finish <= finish) != null;
	}

	protected virtual async Task SetTimeIntervallBlocked(Timespan.Types.Models.BlockedTimeIntervallType reason) {
		if (reason == Timespan.Types.Models.BlockedTimeIntervallType.None) {
			await SetTimeIntervallUnblocked();
			return;
		}
		List<Timespan.Types.Models.Task> tasks = dbService.QueryBlockingTasksInIntervallAsync(TimeIntervallStartSeconds, TimeIntervallStopSeconds).Result;
		ForeachCell(tasks, (row, col, start, finish, task)=>SetCellBlocked(row, col, start, finish, task, reason));
		UpdateColumnMarkers();
	}

	private void SetCellBlocked(int row, int column, long start, long finish, List<Timespan.Types.Models.Task> tasks, Timespan.Types.Models.BlockedTimeIntervallType reason) {
		if (IsMarked[row, column].Value) {
			IEnumerable<Timespan.Types.Models.Task> tasks_ = tasks
				.Where(x => x.start >= start && x.start <= finish)
					.Where(x => x.finish >= start && x.finish <= finish);
			if (!tasks_.Any()) {
				dbService.CreateIntervallBlockingTaskAsync(reason, new DateTime(start * TimeSpan.TicksPerSecond), XAxisSegmentDuration - 1);
			}
		}
	}

	protected virtual async Task SetTimeIntervallUnblocked() {
		List<Timespan.Types.Models.Task> tasks = dbService.QueryBlockingTasksInIntervallAsync(TimeIntervallStartSeconds, TimeIntervallStopSeconds).Result;
		ForeachCell(tasks, SetCellUnblocked);
		UpdateColumnMarkers();
	}

	private void SetCellUnblocked(int row, int column, long start, long finish, List<Timespan.Types.Models.Task> tasks) {
		if (IsMarked[row, column].Value) {
			IEnumerable<Timespan.Types.Models.Task> tasks_ = tasks
				.Where(x => x.start >= start && x.start <= finish)
					.Where(x => x.finish >= start && x.finish <= finish);
			foreach (var task in tasks_)
				dbService.DeleteTaskAsync(task);
		}
	}

	protected virtual void ForeachCell(List<Timespan.Types.Models.Task> tasks, Action<int, int, long, long, List<Timespan.Types.Models.Task>> callback) {
		long start = TimeIntervallStartSeconds;
		long finish = start + XAxisSegmentDuration;
		for (int row = 0; row < YAxisSegmentCount; row++) {
			for (int column = 0; column < XAxisSegmentCount; column++) {
				callback(row, column, start, finish, tasks);
				start += XAxisSegmentDuration;
				finish += XAxisSegmentDuration;
			}
		}
	}

	#endregion
}
