using CommunityToolkit.Mvvm.ComponentModel;

using System.Linq;
using System.Threading.Tasks;

using Timespan.Database.Services;
using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Graphs;

public partial class DayViewModel : ViewModelBase, IGraphsViewChild {

	public GUI.Services.CacheService CacheService;
	private IHourglassDbService dbService;

	//[ObservableProperty]
 //   private GridLength[] columnWidths = new[]
 //   {
 //       new GridLength(1, GridUnitType.Star),
 //       new GridLength(1, GridUnitType.Pixel)
 //   };

	[ObservableProperty]
	private bool[] markedColumns;

	[ObservableProperty]
	private bool[] blockedColumns;

	[ObservableProperty]
	private long xAxisSegmentDuration;

	[ObservableProperty]
	private long xAxisSegmentCount;

	[ObservableProperty]
	private long timeIntervallStartSeconds;

	[ObservableProperty]
	private long timeIntervallStopSeconds;

	public DayViewModel() : this(null, null) {
	}

    public DayViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService) : base() {
		CacheService = cacheService;
		this.dbService = dbService;
		MarkedColumns = new bool[32];
		BlockedColumns = new bool[32];
		for (int i = 0; i < MarkedColumns.Length; i++) {
			MarkedColumns[i] = false;
		}
		xAxisSegmentDuration = 3600;
		xAxisSegmentCount = 24;
		//for(int i=0; i<showColumn.Length; i++) {
		//	if (i < 6) { 
		//		showColumn[i] = new(0, GridUnitType.Star);
		//	} else if(i > 17) {
		//		showColumn[i] = new(0, GridUnitType.Star);
		//	} else {
		//		showColumn[i] = new(1, GridUnitType.Star);
		//	}
		//};
	}

	public string GetDateString() {
		return "Montag 14.3.";
	}

	public async Task GetTasksAsync() {

	}

	public void UpdateColumnMarkers() {
		long start = TimeIntervallStartSeconds;
		long finish = start + XAxisSegmentDuration;
		List<Timespan.Types.Models.Task> tasks = dbService.QueryBlockingTasksInIntervallAsync(TimeIntervallStartSeconds, TimeIntervallStopSeconds).Result;
		for (int i = 0; i < XAxisSegmentCount; i++) {
			BlockedColumns[i] = tasks
				.Where(x => x.start >= start && x.start <= finish)
					.FirstOrDefault(x => x.finish >= start && x.finish <= finish) != null;
			start += XAxisSegmentDuration;
			finish += XAxisSegmentDuration;
		}
	}

	public void OnLoad() {
		UpdateColumnMarkers();
	}

	public async Task OnMissingContextMenuClicked(Timespan.Types.Models.BlockedTimeIntervallType reason) {
		await SetTimeIntervallBlocked(reason);
		UpdateColumnMarkers();
	}

	public void OnMousePressed(bool isLeftDown, bool isRightdown) {
		if (!isRightdown)
			for (int i = 0; i < xAxisSegmentCount; i++)
				MarkedColumns[i] = false;
	}

	public void OnMouseDragging(Avalonia.Rect dragRect, double width, double paddingX) {
		double leftRectBound = dragRect.X - paddingX;
		double rightRectBound = leftRectBound + dragRect.Width;
		for (int i = 0; i < XAxisSegmentCount; i++) {
			double leftSegmentBound = width * i / XAxisSegmentCount;
			double rightSegmentBound = width * (i + 1) / XAxisSegmentCount;
			MarkedColumns[i] = false;
			if (rightRectBound < leftSegmentBound)
				continue;
			if (leftRectBound > rightSegmentBound)
				continue;
			MarkedColumns[i] = true;
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
			if (MarkedColumns[i]) {
				IEnumerable<Timespan.Types.Models.Task> tasks_ = tasks
					.Where(x => x.start >= start && x.start <= finish)
						.Where(x => x.finish >= start && x.finish <= finish);
				if (!tasks_.Any()) {
					await dbService.CreateIntervallBlockingTaskAsync(reason, new DateTime(start * TimeSpan.TicksPerSecond), xAxisSegmentDuration);
				}
			}
			start += XAxisSegmentDuration;
			finish += XAxisSegmentDuration;
		}
	}

	public async Task SetTimeIntervallUnblocked() {
		long start = TimeIntervallStartSeconds;
		long finish = start + XAxisSegmentDuration;
		List<Timespan.Types.Models.Task> tasks = dbService.QueryBlockingTasksInIntervallAsync(TimeIntervallStartSeconds, TimeIntervallStopSeconds).Result;
		for (int i = 0; i < XAxisSegmentCount; i++) {
			if (MarkedColumns[i]) {
				IEnumerable<Timespan.Types.Models.Task> tasks_ = tasks
					.Where(x => x.start >= start && x.start <= finish)
						.Where(x => x.finish >= start && x.finish <= finish);
				foreach (var task in tasks_)
					await dbService.DeleteTaskAsync(task);
			}
			start += XAxisSegmentDuration;
			finish += XAxisSegmentDuration;
		}
	}

	public virtual void OnTaskClicked(Timespan.Types.Models.Task task) {
		CacheService.SelectedTask = task;
		GlobalEventService.Raise(new ShowTaksEventArgs(task));
	}
}
