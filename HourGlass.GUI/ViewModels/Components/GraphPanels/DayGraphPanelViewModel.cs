namespace Hourglass.GUI.ViewModels.Components.GraphPanels;

using Timespan.Database.Services.Interfaces;
using Hourglass.GUI.Services;
using Hourglass.GUI.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DayGraphPanelViewModel : GraphPanelViewModelBase {

    public override int TASK_GRAPH_COLUMN_COUNT => 1;

    public override int MAX_TASKS => 5;

    public override int GRAPH_CLICK_ADDITIONAL_WIDTH => 5;

    public override int GRAPH_CLICK_ADDITIONAL_HEIGHT => 2;

    public override int GRAPH_MINIMAL_WIDTH => 8;

    public override int GRAPH_CORNER_RADIUS => 12;

    public override long TIME_INTERVALL_START_SECONDS => DateTimeService.ToSeconds(DateTimeService.FloorDay(cacheService.SelectedDay));
    public override long X_AXIS_SEGMENT_DURATION => TimeSpan.SecondsPerHour;

    public override int X_AXIS_SEGMENT_COUNT => 24;
    public override int Y_AXIS_SEGMENT_COUNT => MAX_TASKS;

    public override double TASK_DESCRIPTION_GRAPH_SPACE => 10;
    public override double TASK_DESCRIPTION_FONT_SIZE => 30;

    public DayGraphPanelViewModel() : this(null, null, null, null, null) {

	}

	public DayGraphPanelViewModel(ComponentViewModelFactory<TaskGraphViewModel> graphFactory, IHourglassDbService dbService, DateTimeService dateTimeService, MainViewModel pageController, CacheService cacheService)
		: this(graphFactory, dbService, dateTimeService, null, pageController, cacheService) { }

	public DayGraphPanelViewModel(ComponentViewModelFactory<TaskGraphViewModel> graphFactory, IHourglassDbService dbService, DateTimeService dateTimeService, GraphPageViewModel panelController, MainViewModel pageController, CacheService cacheService)
		: base(graphFactory, dbService, dateTimeService, panelController, pageController, cacheService) { }

	public async override Task<List<Database.Types.Task>> GetTasksAsync() =>
		dbService != null ? await dbService.QueryTasksOfDayAtDateAsync(cacheService.SelectedDay) : [];

	public override void OnDoubleClick(DateTime clickedTime) {
		Console.WriteLine("day graph panel model double click");
	}

	protected override string GetTitle() {
		string day = cacheService.SelectedDay.DayOfWeek switch {
			DayOfWeek.Monday => TranslatorService.Singleton["Days.Monday"] ?? "Monday",
			DayOfWeek.Tuesday => TranslatorService.Singleton["Days.Tuesday"] ?? "Tuesday",
			DayOfWeek.Wednesday => TranslatorService.Singleton["Days.Wednesday"] ?? "Wednesday",
			DayOfWeek.Thursday => TranslatorService.Singleton["Days.Thursday"] ?? "Thursday",
			DayOfWeek.Friday => TranslatorService.Singleton["Days.Friday"] ?? "Friday",
			DayOfWeek.Saturday => TranslatorService.Singleton["Days.Saturday"] ?? "Saturday",
			DayOfWeek.Sunday => TranslatorService.Singleton["Days.Sunday"] ?? "Sunday",
            _ => "Unknown"
		};
		string date = DateTimeService.ToDayAndMonthString(cacheService.SelectedDay);
		return $"{day}  {date}";
	}

    protected override void PreviusIntervallClick() {
        cacheService.SelectedDay = cacheService.SelectedDay.AddDays(-1);
    }

    protected override void FollowingIntervallClick() {
        cacheService.SelectedDay = cacheService.SelectedDay.AddDays(1);
    }
}