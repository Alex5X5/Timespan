using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DynamicData;

using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Services.Mapping;
using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Graphs;

public abstract partial class GraphPanelViewModelBase : ViewModelBase, IGraphsViewChild, IGraphViewModel {

	protected Services.CacheService cacheService;
	protected IHourglassDbService dbService;
	protected SettingsService settingsService;

	#region observable properties

	[ObservableProperty]
	public partial ObservableCollection<ObservableBool> MarkedRows { set; get; }

	[ObservableProperty]
	public partial ObservableCollection<ObservableBool> BlockedRows { set; get; }

	[ObservableProperty]
	public partial ObservableCollection<ObservableBool> MarkedColumns { set; get; }

	[ObservableProperty]
	public partial ObservableCollection<ObservableBool> BlockedColumns { set; get; }

	[ObservableProperty]
	public partial ObservableCollection<ObservableTask> Tasks { set; get; }

	[ObservableProperty]
	private bool[,] isTodaySegment = new bool[0,0];


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

	#endregion

	public GraphPanelViewModelBase(Services.CacheService cacheService, IHourglassDbService dbService, SettingsService settingsService, long start, long finish, int rows=1, int columns=24, int taskRows=1, int taskColumns=1, long duration=3600) : base() {
		this.cacheService = cacheService;
		this.dbService = dbService;
		this.settingsService = settingsService;
		TimeIntervallStartSeconds = start;
		TimeIntervallStopSeconds = finish;
		XAxisSegmentDuration = duration;
		MarkedRows = new();
		BlockedRows = new();
		MarkedColumns = new();
		BlockedColumns = new();
		XAxisSegmentCount = columns;
		YAxisSegmentCount = rows;
		TaskGridRowCount = taskRows;
		TaskGridColumnCount = taskColumns;
		IsTodaySegment = new bool[YAxisSegmentCount, XAxisSegmentCount];
		for (int row = 0; row < YAxisSegmentCount; row++)
			for (int column = 0; column < XAxisSegmentCount; column++) {
				IsTodaySegment[row, column] = IsToday(row, column);
			}
		for (int i = 0; i < MarkedRows.Count; i++) {
			MarkedRows[i] = new(false);
			MarkedRows[i] = new(false);
		}
		for (int i = 0; i < BlockedRows.Count; i++) {
			BlockedRows[i] = new(false);
			BlockedRows[i] = new(false);
		}
		for (int i = 0; i < MarkedColumns.Count; i++) {
			MarkedColumns[i] = new(false);
			MarkedColumns[i] = new(false);
		}
		for (int i = 0; i < BlockedColumns.Count; i++) {
			BlockedColumns[i] = new(false);
			BlockedColumns[i] = new(false);
		}
		Tasks = new();
	}

	public abstract string GetDateString();

	protected abstract bool IsToday(int ro, int column);

	protected abstract GridCellPosition GetCellForTask(ObservableTask task);

	public virtual async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return dbService != null ? await dbService.QueryTasksOfDayAtDateAsync(DateTimeService.FloorDay(cacheService.SelectedDay)) : [];
	}

	#region commands

	[RelayCommand]
	protected virtual void OnLoad() {
		UpdateColumnMarkers();
		GlobalEventService.Subscribe<IntervallChangedEventArgs>(OnIntervallChanged);
		GlobalEventService.Subscribe<TasksChangedEventArgs>(OnTasksChanged);
		GlobalEventService.Subscribe<ShowTaksEventArgs>(OnShowTask);
		GlobalEventService.Raise<IntervallChangedEventArgs>();
		GlobalEventService.Raise<TasksChangedEventArgs>();
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
	protected virtual void OnMissingContextMenuClicked(Timespan.Types.Models.BlockedTimeIntervallType reason) {
		SetTimeIntervallBlocked(reason);
	}

	[RelayCommand]
	protected virtual void OnMousePressed(MousePressedEventArgs args) {
		if (!args.Right)
			for (int i = 0; i < XAxisSegmentCount; i++)
				MarkedColumns[i].Value = false;
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
		SelectedDay = cacheService.SelectedDay;
		TimeIntervallStartSeconds = DateTimeService.ToSeconds(FloorIntervall(cacheService.SelectedDay));
		TimeIntervallStopSeconds = DateTimeService.ToSeconds(CeilIntervall(cacheService.SelectedDay));
		UpdateColumnMarkers();
	}

	private async void OnTasksChanged(TasksChangedEventArgs args) {
		List<Timespan.Types.Models.Task> tasks = await GetTasksAsync();
		await Task.Run(() => {
			var tasks_ = tasks.Select(TaskMapper.ToDomain).ToList();
			if (tasks_.Count == 0)
				Tasks = [];
			else
				Tasks = new(tasks_);
		});
	}

	private void OnShowTask(ShowTaksEventArgs args) {
		 
	}

	protected abstract DateTime FloorIntervall(DateTime date);
	protected abstract DateTime CeilIntervall(DateTime date);

	partial void OnXAxisSegmentCountChanged(long value) {
		if (MarkedColumns != null) {
			while (MarkedColumns.Count > value)
				MarkedColumns.RemoveAt(MarkedColumns.Count - 1);
			while (MarkedColumns.Count < value)
				MarkedColumns.Add(new ObservableBool(false));
		}
		if (BlockedColumns != null) {
			while (BlockedColumns.Count > value)
				BlockedColumns.RemoveAt(BlockedColumns.Count - 1);
			while (BlockedColumns.Count < value)
				BlockedColumns.Add(new ObservableBool(false));
		}
	}

	partial void OnYAxisSegmentCountChanged(long value) {
		if (MarkedRows != null) {
			while (MarkedRows.Count > value)
				MarkedRows.RemoveAt(MarkedRows.Count - 1);
			while (MarkedRows.Count < value)
				MarkedRows.Add(new ObservableBool(false));
		}
		if (BlockedRows != null) {
			while (BlockedRows.Count > value)
				BlockedRows.RemoveAt(BlockedRows.Count - 1);
			while (BlockedRows.Count < value)
				BlockedRows.Add(new ObservableBool(false));
		}
	}

	#endregion

	#region marked rows and columns

	protected virtual void UpdateColumnMarkers() {
		long start = TimeIntervallStartSeconds;
		long finish = start + XAxisSegmentDuration;
		List<Timespan.Types.Models.Task> tasks = dbService.QueryBlockingTasksInIntervallAsync(TimeIntervallStartSeconds, TimeIntervallStopSeconds).Result;
		for (int i = 0; i < XAxisSegmentCount; i++) {
			BlockedColumns[i].Value = tasks
				.Where(x => x.start >= start && x.start <= finish)
					.FirstOrDefault(x => x.finish >= start && x.finish <= finish) != null;
			start += XAxisSegmentDuration;
			finish += XAxisSegmentDuration;
		}
	}

	protected virtual async Task SetTimeIntervallBlocked(Timespan.Types.Models.BlockedTimeIntervallType reason) {
		if (reason == Timespan.Types.Models.BlockedTimeIntervallType.None) {
			await SetTimeIntervallUnblocked();
			return;
		}
		long start = TimeIntervallStartSeconds;
		long finish = start + XAxisSegmentDuration;
		List<Timespan.Types.Models.Task> tasks = dbService.QueryBlockingTasksInIntervallAsync(TimeIntervallStartSeconds, TimeIntervallStopSeconds).Result;
		for (int i = 0; i < XAxisSegmentCount; i++) {
			if (MarkedColumns[i].Value) {
				IEnumerable<Timespan.Types.Models.Task> tasks_ = tasks
					.Where(x => x.start >= start && x.start <= finish)
						.Where(x => x.finish >= start && x.finish <= finish);
				if (!tasks_.Any()) {
					await dbService.CreateIntervallBlockingTaskAsync(reason, new DateTime(start * TimeSpan.TicksPerSecond), XAxisSegmentDuration);
				}
			}
			start += XAxisSegmentDuration;
			finish += XAxisSegmentDuration;
		}
		UpdateColumnMarkers();
	}

	protected virtual async Task SetTimeIntervallUnblocked() {
		long start = TimeIntervallStartSeconds;
		long finish = start + XAxisSegmentDuration;
		List<Timespan.Types.Models.Task> tasks = dbService.QueryBlockingTasksInIntervallAsync(TimeIntervallStartSeconds, TimeIntervallStopSeconds).Result;
		for (int i = 0; i < XAxisSegmentCount; i++) {
			if (MarkedColumns[i].Value) {
				IEnumerable<Timespan.Types.Models.Task> tasks_ = tasks
					.Where(x => x.start >= start && x.start <= finish)
						.Where(x => x.finish >= start && x.finish <= finish);
				foreach (var task in tasks_)
					await dbService.DeleteTaskAsync(task);
			}
			start += XAxisSegmentDuration;
			finish += XAxisSegmentDuration;
		}
		UpdateColumnMarkers();
	}

	#endregion
}
