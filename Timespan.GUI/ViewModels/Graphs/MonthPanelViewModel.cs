using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Graphs;

public partial class MonthPanelViewModel : GraphPanelViewModelBase {

	public MonthPanelViewModel() : this(null, null) {

	}

	public MonthPanelViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService) : base(cacheService, dbService, 1, 24, 3600) {
		
	}

	public override string GetDateString() {
		string month = TranslatorService.Singleton.TranslateMonth(CacheService.SelectedDay.Month);
		return $"{month} {CacheService.SelectedDay.Year}";
	}

	public async override Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return [];
	}
}
