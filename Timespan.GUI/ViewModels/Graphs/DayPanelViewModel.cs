using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Graphs;

public partial class DayPanelViewModel : GraphPanelViewModelBase {

	public DayPanelViewModel() : this(null, null) {
		
	}

    public DayPanelViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService) : base(cacheService, dbService, 1, 24, 3600) {
		
	}

	public override string GetDateString() {
		string day = TranslatorService.Singleton.TranslateDayShort(CacheService.SelectedDay.DayOfWeek);
		string month = TranslatorService.Singleton.TranslateMonthShort(CacheService.SelectedDay.Month);
		return $"{day}. {CacheService.SelectedDay.Day}. {month}. {CacheService.SelectedDay.Year}";
	}

	public async override Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return [];
	}
}
