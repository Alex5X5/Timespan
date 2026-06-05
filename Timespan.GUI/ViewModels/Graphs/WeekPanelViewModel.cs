using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;

namespace Timespan.GUI.ViewModels.Graphs;

public partial class WeekPanelViewModel : GraphPanelViewModelBase {

	public WeekPanelViewModel() : this(null, null) {

	}

	public WeekPanelViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService) : base(cacheService, dbService, 1, 5, 86400) {

	}

	public override string GetDateString() {
		return "Kw 2 ";
	}

	public async override Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return [];
	}
}
