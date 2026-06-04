using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Graphs;

public partial class MonthViewModel : GraphViewModelBase {

	public MonthViewModel() : this(null, null) {

	}

	public MonthViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService) : base(cacheService, dbService, 1, 24, 3600) {

	}

	public override string GetDateString() {
		string month = TranslatorService.Singleton.TranslateMonth(CacheService.SelectedDay.Month);
		return $"{month} {CacheService.SelectedDay.Year}";
	}

	public async override Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return [];
	}
}
