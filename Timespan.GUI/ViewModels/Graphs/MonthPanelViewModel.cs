namespace Timespan.GUI.ViewModels.Graphs;

using Avalonia.Media;

using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

public partial class MonthPanelViewModel : GraphPanelViewModelBase {

	public MonthPanelViewModel() : this(null, null) {

	}

	public MonthPanelViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService) : base(
			cacheService, dbService,
			DateTimeService.ToSeconds(DateTimeService.FloorWeek(cacheService.SelectedDay)),
			DateTimeService.ToSeconds(DateTimeService.CeilWeek(cacheService.SelectedDay)),
			5, DateTimeService.WeeksInMonth(cacheService.SelectedDay), 86400) {
		
	}

	protected override bool IsToday(int row, int column) {
		if(cacheService.SelectedDay.Month != DateTime.Today.Month)
			return false;
		int offset = DateTimeService.DayOfWorkWeek(DateTimeService.GetFirstDayOfMonthAtDate(cacheService.SelectedDay)) - 1;
		return row * column == DateTime.Today.Day + offset;
	}

	public override string GetDateString() {
		string month = TranslatorService.Singleton.TranslateMonth(cacheService.SelectedDay.Month);
		return $"{month} {cacheService.SelectedDay.Year}";
	}

	protected override void PreviousIntervallClick() {
		cacheService.SelectedDay = DateTimeService.FloorMonth(cacheService.SelectedDay.AddMonths(-1));
	}

	protected override void FollowingIntervallClick() {
		cacheService.SelectedDay = DateTimeService.FloorMonth(cacheService.SelectedDay.AddMonths(1));
	}

	protected override void SelectedDayChanged(IntervallChangedEventArgs args) {
		TimeIntervallStartSeconds = DateTimeService.ToSeconds(DateTimeService.FloorDay(cacheService.SelectedDay));
		TimeIntervallStopSeconds = DateTimeService.ToSeconds(DateTimeService.CeilDay(cacheService.SelectedDay));
		YAxisSegmentCount = DateTimeService.WeeksInMonth(cacheService.SelectedDay);
		base.SelectedDayChanged(args);
	}

	public override async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return dbService != null ? await dbService.QueryTasksOfMonthAtDateAsync(DateTimeService.FloorMonth(cacheService.SelectedDay)) : [];
	}
}
