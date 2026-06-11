namespace Timespan.GUI.ViewModels.Graphs;

using Avalonia.Media;

using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

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
		if (row != 0)
			return false;
		if (DateTimeService.DayOfWorkWeek(DateTime.Today) == column)
			return true;
		return false;
	}

	public override string GetDateString() {
		return "Kw 2 ";
	}

	protected override void PreviousIntervallClick() {
		cacheService.SelectedDay = DateTimeService.FloorWeek(cacheService.SelectedDay.AddDays(-7));
	}

	protected override void FollowingIntervallClick() {
		cacheService.SelectedDay = DateTimeService.FloorWeek(cacheService.SelectedDay.AddDays(7));
	}

	protected override void SelectedDayChanged(IntervallChangedEventArgs args) {
		TimeIntervallStartSeconds = DateTimeService.ToSeconds(DateTimeService.FloorWeek(cacheService.SelectedDay));
		TimeIntervallStopSeconds = DateTimeService.ToSeconds(DateTimeService.CeilWeek(cacheService.SelectedDay));
		base.SelectedDayChanged(args);
	}

	public override async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return dbService != null ? await dbService.QueryTasksOfWeekAtDateAsync(DateTimeService.FloorWeek(cacheService.SelectedDay)) : [];
	}
}
