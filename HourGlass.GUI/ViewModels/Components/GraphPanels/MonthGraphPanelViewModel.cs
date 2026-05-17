namespace Hourglass.GUI.ViewModels.Components.GraphPanels;

using Timespan.Database.Services.Interfaces;
using Hourglass.GUI.Services;
using Hourglass.GUI.ViewModels.Pages;
using Hourglass.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MonthGraphPanelViewModel : GraphPanelViewModelBase {

    public override int TASK_GRAPH_COLUMN_COUNT => 2;

    public override int MAX_TASKS => 70;

    public override int GRAPH_CLICK_ADDITIONAL_WIDTH => 6;
    public override int GRAPH_CLICK_ADDITIONAL_HEIGHT => 4;

    public override int GRAPH_MINIMAL_WIDTH => 2;
    public override int GRAPH_CORNER_RADIUS => 4;

    public override long TIME_INTERVALL_START_SECONDS => DateTimeService.ToSeconds(DateTimeService.FloorMonth(cacheService?.SelectedDay ?? DateTime.Now));
    public override long X_AXIS_SEGMENT_DURATION => TimeSpan.SecondsPerDay;

    public override int X_AXIS_SEGMENT_COUNT =>
        DateTime.DaysInMonth(
            cacheService.SelectedDay.Year,
            cacheService.SelectedDay.Month
        );
    public override int Y_AXIS_SEGMENT_COUNT => MAX_TASKS;

    public override double TASK_DESCRIPTION_GRAPH_SPACE => 5;
    public override double TASK_DESCRIPTION_FONT_SIZE => 10;

    public MonthGraphPanelViewModel() : this(null, null, null, null, null) {

	}

	public MonthGraphPanelViewModel(ComponentViewModelFactory<TaskGraphViewModel> graphFactory, IHourglassDbService dbService, DateTimeService dateTimeService, MainViewModel pageController, CacheService cacheService)
        : base(graphFactory, dbService, dateTimeService, null, pageController, cacheService) { }


    public async override Task<List<Database.Models.Task>> GetTasksAsync() =>
		dbService != null ? await dbService.QueryTasksOfMonthAtDateAsync(cacheService.SelectedDay) : [];

	//public override void OnClick(Database.Models.Task task) {
	//	pageController.ChangePage<TaskDetailsPageViewModel>();
	//	Console.WriteLine("month graph panel model click");
	//}

	public override void OnDoubleClick(DateTime clickedTime) {
		Console.WriteLine("month graph panel model double click");
		if (dateTimeService != null)
			cacheService.SelectedDay = DateTimeService.FloorWeek(clickedTime);
		panelController?.ChangeGraphPanel<WeekGraphPanelViewModel>();
	}

	protected override string GetTitle() {
		string month = cacheService.SelectedDay.Month switch {
			1 => TranslatorService.Singleton["Months.January"] ?? "January",
			2 => TranslatorService.Singleton["Months.February"] ?? "February",
			3 => TranslatorService.Singleton["Months.March"] ?? "March",
			4 => TranslatorService.Singleton["Months.April"] ?? "April",
			5 => TranslatorService.Singleton["Months.May"] ?? "May",
			6 => TranslatorService.Singleton["Months.June"] ?? "June",
			7 => TranslatorService.Singleton["Months.July"] ?? "July",
			8 => TranslatorService.Singleton["Months.August"] ?? "August",
			9 => TranslatorService.Singleton["Months.September"] ?? "September",
			10 => TranslatorService.Singleton["Months.October"] ?? "October",
			11 => TranslatorService.Singleton["Months.November"] ?? "November",
			12 => TranslatorService.Singleton["Months.December"] ?? "December",
			_ => "Unknown"
		};
		return $"{month}  {cacheService.SelectedDay.Year}";
    }

    protected override void PreviusIntervallClick() {
		DateTime previousMonth = DateTimeService.FloorMonth(cacheService.SelectedDay).AddDays(-1);
        previousMonth = DateTimeService.FloorMonth(previousMonth);
		cacheService.SelectedDay = previousMonth;
    }

    protected override void FollowingIntervallClick() {
        DateTime nextMonth = DateTimeService.FloorMonth(cacheService.SelectedDay);
        nextMonth = DateTimeService.FloorMonth(nextMonth.AddDays(DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month) + 1));
        cacheService.SelectedDay = nextMonth;
    }
}