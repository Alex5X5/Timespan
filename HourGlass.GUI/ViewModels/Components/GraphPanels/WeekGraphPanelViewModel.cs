namespace Hourglass.GUI.ViewModels.Components.GraphPanels;

using Timespan.Database.Services.Interfaces;
using Hourglass.GUI.Services;
using Hourglass.GUI.ViewModels.Pages;
using Hourglass.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class WeekGraphPanelViewModel : GraphPanelViewModelBase {

	public override int TASK_GRAPH_COLUMN_COUNT => 1;

	public override int MAX_TASKS => 20;

	public override int GRAPH_CLICK_ADDITIONAL_WIDTH => 8;

	public override int GRAPH_CLICK_ADDITIONAL_HEIGHT => 5;

	public override int GRAPH_MINIMAL_WIDTH => 5;

	public override int GRAPH_CORNER_RADIUS => 5;

	public override long TIME_INTERVALL_START_SECONDS => DateTimeService.ToSeconds(DateTimeService.FloorWeek(cacheService?.SelectedDay ?? DateTime.Now));
	public override long X_AXIS_SEGMENT_DURATION => TimeSpan.SecondsPerDay;

	public override int X_AXIS_SEGMENT_COUNT => 7;
	public override int Y_AXIS_SEGMENT_COUNT => MAX_TASKS;

	public override double TASK_DESCRIPTION_GRAPH_SPACE => 5;
	public override double TASK_DESCRIPTION_FONT_SIZE => 10;


	public WeekGraphPanelViewModel() : this(null, null, null, null, null) {

	}

	public WeekGraphPanelViewModel(ComponentViewModelFactory<TaskGraphViewModel> graphFactory, IHourglassDbService dbService, DateTimeService dateTimeService, MainViewModel pageController, CacheService cacheService)
        : base(graphFactory, dbService, dateTimeService, null, pageController, cacheService) { }


    public async override Task<List<Database.Models.Task>> GetTasksAsync() =>
		dbService != null ? await dbService.QueryTasksOfWeekAtDateAsync(cacheService.SelectedDay) : [];

	//public override void OnClick(Database.Models.Task task) {
	//	pageController.ChangePage<TaskDetailsPageViewModel>();
	//	Console.WriteLine("week graph panel model click");
	//}

	public override void OnDoubleClick(DateTime clickedTime) {
		Console.WriteLine("week graph panel model double click");
		if(dateTimeService!=null)
			cacheService.SelectedDay = DateTimeService.FloorDay(clickedTime);
		panelController?.ChangeGraphPanel<DayGraphPanelViewModel>();
	}

	protected override string GetTitle() {
		int week = dateTimeService.GetWeekCountAtDate(cacheService.SelectedDay);
		string startDate = DateTimeService.ToDayAndMonthString(DateTimeService.FloorWeek(cacheService.SelectedDay));
		string endDate = DateTimeService.ToDayAndMonthString(DateTimeService.FloorWeek(cacheService.SelectedDay).AddDays(5));
		return $"W {week}  {startDate}-{endDate}";
	}

    protected override void PreviusIntervallClick() {
		cacheService.SelectedDay = cacheService.SelectedDay.AddDays(-7);
	}

    protected override void FollowingIntervallClick() {
		cacheService.SelectedDay = cacheService.SelectedDay.AddDays(7);
	}
}