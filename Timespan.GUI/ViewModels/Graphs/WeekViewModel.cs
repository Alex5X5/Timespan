using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;

namespace Timespan.GUI.ViewModels.Graphs;

public partial class WeekViewModel : GraphViewModelBase {

	public WeekViewModel() : this(null, null) {

	}

	public WeekViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService) : base(cacheService, dbService, 1, 5, 3600) {

	}

	public override string GetDateString() {
		return "Kw 2 ";
	}

	public async override Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return [];
	}
}
