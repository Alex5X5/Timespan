using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Graphs;

public partial class WeekViewModel : ViewModelBase, IGraphsViewChild {

	public GUI.Services.CacheService CacheService;
	private IHourglassDbService dbService;

	[ObservableProperty]
	public partial ObservableCollection<ObservableBool> MarkedColumns {
		set; get;
	}

	[ObservableProperty]
	public partial ObservableCollection<ObservableBool> BlockedColumns {
		set; get;
	}

	[ObservableProperty]
	private long xAxisSegmentDuration = System.TimeSpan.SecondsPerDay;

	[ObservableProperty]
	private long xAxisSegmentCount = 5;

	[ObservableProperty]
	private long timeIntervallStartSeconds;

	[ObservableProperty]
	private long timeIntervallStopSeconds;

	public WeekViewModel() : this(null, null) {
	}

	public WeekViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService) : base() {
		CacheService = cacheService;
		this.dbService = dbService;
		MarkedColumns = new(new ObservableBool[XAxisSegmentCount]);
		BlockedColumns = new(new ObservableBool[XAxisSegmentCount]);
		for (int i = 0; i < MarkedColumns.Count; i++) {
			MarkedColumns[i] = new(false);
			BlockedColumns[i] = new(false);
		}
		var startDate = DateTimeService.FloorWeek(cacheService.SelectedDay);
		var endDate = DateTimeService.CeilWeek(cacheService.SelectedDay);
		TimeIntervallStartSeconds = DateTimeService.ToSeconds(startDate);
		TimeIntervallStopSeconds = DateTimeService.ToSeconds(endDate);
	}

	public string GetDateString() {
		return "Montag 14.3.";
	}

	public async Task GetTasksAsync() {

	}

	public void OnLoad() {
		UpdateColumnMarkers();
	}

	public void OnMissingContextMenuClicked(Timespan.Types.Models.BlockedTimeIntervallType reason) {
		SetTimeIntervallBlocked(reason);
	}

	public virtual void OnTaskClicked(Timespan.Types.Models.Task task) {
		CacheService.SelectedTask = task;
		GlobalEventService.Raise(new ShowTaksEventArgs(task));
	}

	public void OnClicked() {

	}

	public void OnMousePressed(bool isLeftDown, bool isRightdown) {
		if (!isRightdown)
			for (int i = 0; i < XAxisSegmentCount; i++)
				MarkedColumns[i].Value = false;
	}

	public void OnMouseDragging(Avalonia.Rect dragRect, double width, double paddingX) {
		double leftRectBound = dragRect.X - paddingX;
		double rightRectBound = leftRectBound + dragRect.Width;
		for (int i = 0; i < XAxisSegmentCount; i++) {
			double leftSegmentBound = width * i / XAxisSegmentCount;
			double rightSegmentBound = width * (i + 1) / XAxisSegmentCount;
			MarkedColumns[i].Value = !(rightRectBound < leftSegmentBound | leftRectBound > rightSegmentBound);
		}
	}

	public void UpdateColumnMarkers() {
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

	public async Task SetTimeIntervallBlocked(Timespan.Types.Models.BlockedTimeIntervallType reason) {
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

	public async Task SetTimeIntervallUnblocked() {
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
