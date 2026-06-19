namespace Timespan.GUI.ViewModels.Graphs;

using Avalonia.Media;

using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

public partial class DayPanelViewModel : GraphPanelViewModelBase {

	public DayPanelViewModel() : this(null, null, null) {
		
	}

    public DayPanelViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService, SettingsService settingsService) : base(
			cacheService, dbService, settingsService,
			DateTimeService.ToSeconds(DateTimeService.FloorDay(cacheService.SelectedDay)),
			DateTimeService.ToSeconds(DateTimeService.CeilDay(cacheService.SelectedDay)),
			1, 24,
			1, 1, 3600) {

	}

	protected override bool IsToday(int row, int column) {
		return false;
	}

	protected override GridCellPosition GetCellForTask(ObservableTask task) {
		return new(0, 0);
	}

	public override string GetDateString() {
		string day = TranslatorService.Singleton.TranslateDayShort(cacheService.SelectedDay.DayOfWeek);
		string month = TranslatorService.Singleton.TranslateMonthShort(cacheService.SelectedDay.Month);
		return $"{day}. {cacheService.SelectedDay.Day}. {month}. {cacheService.SelectedDay.Year}";
	}

	protected override DateTime FloorIntervall(DateTime date) {
		return DateTimeService.FloorDay(date);
	}

	protected override DateTime CeilIntervall(DateTime date) {
		return DateTimeService.CeilDay(date);
	}

	protected override void PreviousIntervallClick() {
		cacheService.SelectedDay = DateTimeService.FloorDay(cacheService.SelectedDay.AddDays(-1));
	}

	protected override void FollowingIntervallClick() {
		cacheService.SelectedDay = DateTimeService.FloorDay(cacheService.SelectedDay.AddDays(1));
	}

	public override async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return dbService != null ? await dbService.QueryTasksOfDayAtDateAsync(DateTimeService.FloorDay(cacheService.SelectedDay)) : [];
	}
}
