namespace Timespan.GUI.ViewModels.Graphs;

using Avalonia.Media;

using System.Security.Cryptography.Pkcs;
using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

public partial class DayPanelViewModel : GraphPanelViewModelBase {

	public DayPanelViewModel() : this(null, null, null) {
		
	}

    public DayPanelViewModel(GuiStateService stateService, ITimespanDbService dbService, SettingsService settingsService) : base(
			stateService, dbService, settingsService,
			DateTimeService.ToSeconds(DateTimeService.FloorDay(stateService.SelectedDay)),
			DateTimeService.ToSeconds(DateTimeService.CeilDay(stateService.SelectedDay)),
			1, 24,
			1, 1, 3600) {

	}

	protected override bool IsToday(int row, int column) {
		return false;
	}

	public override string GetDateString() {
		string day = TranslatorService.Singleton.TranslateDayShort(stateService.SelectedDay.DayOfWeek);
		string month = TranslatorService.Singleton.TranslateMonthShort(stateService.SelectedDay.Month);
		return $"{day}. {stateService.SelectedDay.Day}. {month}. {stateService.SelectedDay.Year}";
	}

	protected override DateTime FloorIntervall(DateTime date) {
		return DateTimeService.FloorDay(date);
	}

	protected override DateTime CeilIntervall(DateTime date) {
		return DateTimeService.CeilDay(date);
	}

	public override async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return dbService != null ? await dbService.QueryTasksOfDayAtDateAsync(DateTimeService.FloorDay(stateService.SelectedDay)) : [];
	}

	protected override void OnDoubleClick(DoubleClickedEventArgs args) {
		
	}
}
