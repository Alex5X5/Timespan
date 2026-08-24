namespace Timespan.GUI.ViewModels.Graphs;

using Avalonia.Media;

using CommunityToolkit.Mvvm.ComponentModel;

using System.Linq;
using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

public partial class MonthPanelViewModel : GraphPanelViewModelBase {

	private RedirectionService redirectionService;

	[ObservableProperty]
	private int weekOffset = 0;

	public MonthPanelViewModel() : this(null, null, null, null) {

	}

	public MonthPanelViewModel(
			GuiStateService stateService,
			ITimespanDbService dbService,
			SettingsService settingsService,
			RedirectionService redirectionService)
		: base(
			stateService, dbService, settingsService,
			DateTimeService.ToSeconds(DateTimeService.FloorWeek(stateService.SelectedDay)),
			DateTimeService.ToSeconds(DateTimeService.CeilWeek(stateService.SelectedDay)),
			DateTimeService.WeeksInMonth(stateService.SelectedDay), 5,
			DateTimeService.WeeksInMonth(stateService.SelectedDay), 5, 86400) {
		this.redirectionService = redirectionService;
	}

	protected override bool IsToday(int row, int column) {
		if(stateService.SelectedDay.Month != DateTime.Today.Month)
			return false;
		int offset = DateTimeService.DayOfWorkWeek(DateTimeService.GetFirstDayOfMonthAtDate(stateService.SelectedDay));
		return (row * TaskGridColumnCount + column) == (DateTime.Today.Day + offset);
	}

	public override string GetDateString() {
		string month = TranslatorService.Singleton.TranslateMonth(stateService.SelectedDay.Month);
		return $"{month} {stateService.SelectedDay.Year}";
	}

	protected override DateTime FloorIntervall(DateTime date) {
		return DateTimeService.FloorMonth(date);
	}

	protected override DateTime CeilIntervall(DateTime date) {
		return DateTimeService.CeilMonth(date);
	}

	protected override void OnIntervallChanged(IntervallChangedEventArgs args) {
		stateService.SelectedDay = FloorIntervall(stateService.SelectedDay);
		SelectedDay = stateService.SelectedDay;
		YAxisSegmentCount = DateTimeService.WeeksInMonth(stateService.SelectedDay);
		var start = DateTimeService.FloorWeek(DateTimeService.FloorMonth(stateService.SelectedDay));
		var finish = DateTimeService.CeilWeek(DateTimeService.CeilMonth(stateService.SelectedDay));
		TimeIntervallStartSeconds = DateTimeService.ToSeconds(start);
		TimeIntervallStopSeconds = DateTimeService.ToSeconds(finish);
		WeekOffset = DateTimeService.GetWeekCountAtDate(settingsService.StartDate, SelectedDay);
		UpdateColumnMarkers();
	}

	protected override void ForeachCell(List<Timespan.Types.Models.Task> tasks, Action<int, int, long, long, List<Timespan.Types.Models.Task>> callback) {
		long start = TimeIntervallStartSeconds;
		long finish = start + XAxisSegmentDuration;
		for (int row = 0; row < YAxisSegmentCount; row++) {
			for (int column = 0; column < XAxisSegmentCount; column++) {
				var st = DateTimeService.FromSeconds(start);
				var fsh = DateTimeService.FromSeconds(finish);
				Console.WriteLine($"start of cell is {st} finish is {fsh}");
				callback(row, column, start, finish, tasks);
				start += XAxisSegmentDuration;
				finish += XAxisSegmentDuration;
			}
			start += XAxisSegmentDuration;
			start += XAxisSegmentDuration;
			finish += XAxisSegmentDuration;
			finish += XAxisSegmentDuration;
		}
	}

	public override async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return dbService != null ? await dbService.QueryTasksOfMonthAtDateAsync(DateTimeService.FloorMonth(stateService.SelectedDay)) : [];
	}

	protected override void OnDoubleClick(DoubleClickedEventArgs args) {
		var start = DateTimeService.FloorMonth(stateService.SelectedDay);
		start = DateTimeService.FloorWeek(start);
		start = start.AddDays(args.Row * 7);
		stateService.SelectedDay = start;
		redirectionService.GetAnchor<GraphsViewModel, IGraphsViewChild>()?.ChangeModel<WeekPanelViewModel>();
	}
}
