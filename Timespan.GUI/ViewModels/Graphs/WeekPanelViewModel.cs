namespace Timespan.GUI.ViewModels.Graphs;

using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

public partial class WeekPanelViewModel : GraphPanelViewModelBase {

	private RedirectionService redirectionService;

	public WeekPanelViewModel() : this(null, null, null, null) {

	}

	public WeekPanelViewModel(
			GuiStateService stateService,
			ITimespanDbService dbService,
			SettingsService settingsService,
			RedirectionService redirectionService)
		: base(
			stateService, dbService, settingsService,
			DateTimeService.ToSeconds(DateTimeService.FloorWeek(stateService.SelectedDay)),
			DateTimeService.ToSeconds(DateTimeService.CeilWeek(stateService.SelectedDay)),
			1, 5,
			1, 5, 86400) {
		this.redirectionService = redirectionService;
	}

	protected override bool IsToday(int row, int column) {
		if (row != 0)
			return false;
		if (DateTimeService.FloorWeek(stateService.SelectedDay) != DateTimeService.FloorWeek(DateTime.Today))
			return false;
		if (DateTimeService.DayOfWorkWeek(DateTime.Today) != column)
			return false;
		return true;
	}

	public override string GetDateString() {
		int week = DateTimeService.GetWeekCountAtDate(settingsService.StartDate, stateService.SelectedDay);
		return $"{TranslatorService.Singleton["Views.Pages.Graphs.Labels.Week"]} {week}";
	}

	protected override DateTime FloorIntervall(DateTime date) {
		return DateTimeService.FloorWeek(date);
	}

	protected override DateTime CeilIntervall(DateTime date) {
		return DateTimeService.CeilWeek(date);
	}

	public override async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return dbService != null ? await dbService.QueryTasksOfWeekAtDateAsync(DateTimeService.FloorWeek(stateService.SelectedDay)) : [];
	}

	protected override void OnDoubleClick(DoubleClickedEventArgs args) {
		var start = DateTimeService.FloorWeek(stateService.SelectedDay);
		start = start.AddDays(args.Col);
		stateService.SelectedDay = start;
		redirectionService.GetAnchor<GraphsViewModel, IGraphsViewChild>()?.ChangeModel<DayPanelViewModel>();

	}
}
