using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.Util.Services;

namespace Timespan.GUI.ViewModels.Graphs;

public partial class WeekPanelViewModel : GraphPanelViewModelBase {

	public WeekPanelViewModel() : this(null, null) {

	}

	public WeekPanelViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService) : base(
			cacheService, dbService,
			DateTimeService.ToSeconds(DateTimeService.FloorWeek(cacheService.SelectedDay)),
			DateTimeService.ToSeconds(DateTimeService.CeilWeek(cacheService.SelectedDay)),
			1, 5, 86400) {
	}

	protected override bool IsToday(int row, int column) {
		if(row != 0)
			return false;
		if(DateTimeService.DayOfWorkWeek(DateTime.Today) == column)
			return true;
		return false;
	}

	public override string GetDateString() {
		return "Kw 2 ";
	}

	public override async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return dbService != null ? await dbService.QueryTasksOfDayAtDateAsync(DateTimeService.FloorMonth(cacheService.SelectedDay)) : [];
	}
}
