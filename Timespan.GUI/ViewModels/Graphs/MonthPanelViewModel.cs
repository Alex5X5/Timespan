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

	private DateTimeService dateTimeService;

	[ObservableProperty]
	private int weekOffset = 0;

	public MonthPanelViewModel() : this(null, null, null) {

	}

	public MonthPanelViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService, SettingsService settingsService) : base(
			cacheService, dbService, settingsService,
			DateTimeService.ToSeconds(DateTimeService.FloorWeek(cacheService.SelectedDay)),
			DateTimeService.ToSeconds(DateTimeService.CeilWeek(cacheService.SelectedDay)),
			DateTimeService.WeeksInMonth(cacheService.SelectedDay), 5,
			DateTimeService.WeeksInMonth(cacheService.SelectedDay), 5, 86400) {
		dateTimeService = new(settingsService, cacheService);
	}

	protected override bool IsToday(int row, int column) {
		if(cacheService.SelectedDay.Month != DateTime.Today.Month)
			return false;
		int offset = DateTimeService.DayOfWorkWeek(DateTimeService.GetFirstDayOfMonthAtDate(cacheService.SelectedDay));
		return (row * TaskGridColumnCount + column) == (DateTime.Today.Day + offset);
	}

	public override string GetDateString() {
		string month = TranslatorService.Singleton.TranslateMonth(cacheService.SelectedDay.Month);
		return $"{month} {cacheService.SelectedDay.Year}";
	}

	protected override DateTime FloorIntervall(DateTime date) {
		return DateTimeService.FloorMonth(date);
	}

	protected override DateTime CeilIntervall(DateTime date) {
		return DateTimeService.CeilMonth(date);
	}

	protected override void OnIntervallChanged(IntervallChangedEventArgs args) {
		cacheService.SelectedDay = FloorIntervall(cacheService.SelectedDay);
		SelectedDay = cacheService.SelectedDay;
		YAxisSegmentCount = DateTimeService.WeeksInMonth(cacheService.SelectedDay);
		TimeIntervallStartSeconds = DateTimeService.ToSeconds(FloorIntervall(cacheService.SelectedDay));
		TimeIntervallStopSeconds = DateTimeService.ToSeconds(CeilIntervall(cacheService.SelectedDay));
		WeekOffset = dateTimeService.GetWeekCountAtDate(SelectedDay);
		UpdateColumnMarkers();
	}

	protected override void ForeachCell(List<Timespan.Types.Models.Task> tasks, Action<int, int, long, long, List<Timespan.Types.Models.Task>> callback) {
		long start = TimeIntervallStartSeconds;
		long finish = start + XAxisSegmentDuration;
		for (int row = 0; row < YAxisSegmentCount; row++) {
			for (int column = 0; column < XAxisSegmentCount; column++) {
				callback(row, column, start, finish, tasks);
				start += XAxisSegmentDuration;
				finish += XAxisSegmentDuration;
			}
			start += XAxisSegmentDuration;
			finish += XAxisSegmentDuration;
			start += XAxisSegmentDuration;
			finish += XAxisSegmentDuration;
		}
	}

	public override async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return dbService != null ? await dbService.QueryTasksOfMonthAtDateAsync(DateTimeService.FloorMonth(cacheService.SelectedDay)) : [];
	}
}
