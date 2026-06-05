using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Graphs;

public abstract partial class GraphPanelViewModelBase : ViewModelBase, IGraphsViewChild, IGraphViewModel {

	public Services.CacheService CacheService;
	private IHourglassDbService dbService;

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
	private double extraClickSize;
	
	[ObservableProperty]
	private double minimalWidth;

	[ObservableProperty]
	private double maxTasks;

	[ObservableProperty]
	private long xAxisSegmentDuration;

	[ObservableProperty]
	private long xAxisSegmentCount;

	[ObservableProperty]
	private long yAxisSegmentCount;

	[ObservableProperty]
	private long timeIntervallStartSeconds;

	[ObservableProperty]
	private long timeIntervallStopSeconds;

	[ObservableProperty]
	private DateTime selectedDay;

	partial void OnSelectedDayChanged(DateTime value) {
		SelectedDayChanged();
	}

	#endregion

	public GraphPanelViewModelBase(Services.CacheService cacheService, IHourglassDbService dbService, int rows=1, int columns=24, long duration=3600) : base() {
		CacheService = cacheService;
		this.dbService = dbService;
		MarkedRows = new();
		BlockedRows = new();
		MarkedColumns = new();
		BlockedColumns = new();
		XAxisSegmentCount = columns;
		YAxisSegmentCount = rows;
		XAxisSegmentDuration = duration;
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
		TimeIntervallStartSeconds = DateTimeService.ToSeconds(DateTimeService.FloorDay(cacheService.SelectedDay));
		TimeIntervallStopSeconds = DateTimeService.ToSeconds(DateTimeService.CeilDay(cacheService.SelectedDay));
	}

	public abstract string GetDateString();

	public virtual async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		long start = TimeIntervallStartSeconds;
		long stop = start + TimeIntervallStopSeconds;
		return dbService != null ? await dbService.QueryBlockingTasksInIntervallAsync(TimeIntervallStartSeconds, TimeIntervallStopSeconds) : [];
	}

	#region relay commands

	[RelayCommand]
	protected virtual void OnLoad() {
		UpdateColumnMarkers();
		CacheService.OnSelectedDayChanged += UpdateSelectedDay;
	}

	[RelayCommand]
	protected virtual void OnUnload() {
		CacheService.OnSelectedDayChanged -= UpdateSelectedDay;
	}

	private void UpdateSelectedDay(DateTime? date) {
		UpdateColumnMarkers();
		SelectedDay = date ?? SelectedDay;
	}

	[RelayCommand]
	protected virtual void OnClicked() {
		
	}

	[RelayCommand]
	protected virtual void OnTaskClicked(TaskClickedEventArgs args) {
		CacheService.SelectedTask = args.Task;
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

	protected virtual void SelectedDayChanged() {
		
	}

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
