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

    public GraphPanelViewModelBase(Services.CacheService cacheService, IHourglassDbService dbService, int rows=1, int columns=24, long duration=3600) : base() {
		CacheService = cacheService;
		this.dbService = dbService;
		YAxisSegmentCount = rows;
		XAxisSegmentCount = columns;
		XAxisSegmentDuration = duration;
		MarkedRows = new(new ObservableBool[rows]);
		BlockedRows = new(new ObservableBool[rows]);
		MarkedColumns = new(new ObservableBool[columns]);
		BlockedColumns = new(new ObservableBool[columns]);
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

	public abstract Task<List<Timespan.Types.Models.Task>> GetTasksAsync();

	[RelayCommand]
	public void Load() {
		UpdateColumnMarkers();
	}

	[RelayCommand]
	public void OnClicked() {

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
		double leftRectBound = args.DragRectangle.X - args.PaddingX;
		double rightRectBound = leftRectBound + args.DragRectangle.Width;
		for (int i = 0; i < XAxisSegmentCount; i++) {
			double leftSegmentBound = args.Width * i / XAxisSegmentCount;
			double rightSegmentBound = args.Width * (i + 1) / XAxisSegmentCount;
			MarkedColumns[i].Value = !( rightRectBound < leftSegmentBound | leftRectBound > rightSegmentBound );
		}
	}

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
}
