using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Graphs;

public partial class DayPanelViewModel : GraphPanelViewModelBase {

	public DayPanelViewModel() : this(null, null) {
		
	}

    public DayPanelViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService) : base(
			cacheService, dbService,
			DateTimeService.ToSeconds(DateTimeService.FloorDay(cacheService.SelectedDay)),
			DateTimeService.ToSeconds(DateTimeService.CeilDay(cacheService.SelectedDay)),
			1, 24, 3600) {
		
	}

	public override string GetDateString() {
		string day = TranslatorService.Singleton.TranslateDayShort(cacheService.SelectedDay.DayOfWeek);
		string month = TranslatorService.Singleton.TranslateMonthShort(cacheService.SelectedDay.Month);
		return $"{day}. {cacheService.SelectedDay.Day}. {month}. {cacheService.SelectedDay.Year}";
	}

	protected override bool IsToday(int row, int column) {
		return false;
	}

	public override async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return dbService != null ? await dbService.QueryTasksOfDayAtDateAsync(DateTimeService.FloorDay(cacheService.SelectedDay)) : [];
	}
}
