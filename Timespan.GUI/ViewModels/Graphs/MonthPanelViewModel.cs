using Avalonia.Media;

using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Graphs;

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

	public override async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return dbService != null ? await dbService.QueryTasksOfDayAtDateAsync(DateTimeService.FloorWeek(cacheService.SelectedDay)) : [];
	}

	protected override void SelectedDayChanged() {
		base.SelectedDayChanged();
		YAxisSegmentCount = DateTimeService.WeeksInMonth(SelectedDay);
	}
}
