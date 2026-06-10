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

	public override string GetDateString() {
		string month = TranslatorService.Singleton.TranslateMonth(CacheService.SelectedDay.Month);
		return $"{month} {CacheService.SelectedDay.Year}";
	}

	public override async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return dbService != null ? await dbService.QueryTasksOfDayAtDateAsync(DateTimeService.FloorWeek(CacheService.SelectedDay)) : [];
	}

	protected override void SelectedDayChanged() {
		base.SelectedDayChanged();
		YAxisSegmentCount = DateTimeService.WeeksInMonth(SelectedDay);
	}
}
