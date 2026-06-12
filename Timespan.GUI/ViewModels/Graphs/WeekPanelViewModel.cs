namespace Timespan.GUI.ViewModels.Graphs;

using Avalonia.Media;

using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

public partial class WeekPanelViewModel : GraphPanelViewModelBase {

	public WeekPanelViewModel() : this(null, null, null) {

	}

	public WeekPanelViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService, SettingsService settingsService) : base(
			cacheService, dbService, settingsService,
			DateTimeService.ToSeconds(DateTimeService.FloorWeek(cacheService.SelectedDay)),
			DateTimeService.ToSeconds(DateTimeService.CeilWeek(cacheService.SelectedDay)),
			1, 5,
			1, 5, 86400) {
		this.settingsService = settingsService;
	}

	protected override bool IsToday(int row, int column) {
		if (row != 0)
			return false;
		if (DateTimeService.FloorWeek(cacheService.SelectedDay) != DateTimeService.FloorWeek(DateTime.Today))
			return false;
		if (DateTimeService.DayOfWorkWeek(DateTime.Today) != column)
			return false;
		return true;
	}

	protected override GridCellPosition GetCellForTask(ObservableTask task) {
		return new(0, DateTimeService.DayOfWorkWeek(task.StartDateTime));
	}

	public override string GetDateString() {
		int week = new DateTimeService(settingsService, cacheService).GetWeekCountAtDate(cacheService.SelectedDay);
		return $"{TranslatorService.Singleton["Views.Pages.Graphs.Labels.Week"]} {week}";
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
