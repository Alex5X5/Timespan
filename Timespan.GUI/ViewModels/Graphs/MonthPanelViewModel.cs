namespace Timespan.GUI.ViewModels.Graphs;

using Avalonia.Media;

using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

public partial class MonthPanelViewModel : GraphPanelViewModelBase {

	public MonthPanelViewModel() : this(null, null, null) {

	}

	public MonthPanelViewModel(GUI.Services.CacheService cacheService, IHourglassDbService dbService, SettingsService settingsService) : base(
			cacheService, dbService, settingsService,
			DateTimeService.ToSeconds(DateTimeService.FloorWeek(cacheService.SelectedDay)),
			DateTimeService.ToSeconds(DateTimeService.CeilWeek(cacheService.SelectedDay)),
			DateTimeService.WeeksInMonth(cacheService.SelectedDay), 5,
			DateTimeService.WeeksInMonth(cacheService.SelectedDay), 5, 86400) {
		
	}

	protected override bool IsToday(int row, int column) {
		if(cacheService.SelectedDay.Month != DateTime.Today.Month)
			return false;
		int offset = DateTimeService.DayOfWorkWeek(DateTimeService.GetFirstDayOfMonthAtDate(cacheService.SelectedDay));
		return (row * TaskGridColumnCount + column) == (DateTime.Today.Day + offset);
	}

	protected override GridCellPosition GetCellForTask(ObservableTask task) {
		DateTime firstWeek = DateTimeService.FloorWeek(DateTimeService.FloorMonth(task.StartDateTime));
		DateTime taskWeek = DateTimeService.FloorWeek(task.StartDateTime);
		long diffSeconds = DateTimeService.ToSeconds(firstWeek) - DateTimeService.ToSeconds(taskWeek);
		int row = (int)Math.Floor((double)(diffSeconds / 604800));
		return new(row, DateTimeService.DayOfWorkWeek(task.StartDateTime));
	}

	public override string GetDateString() {
		string month = TranslatorService.Singleton.TranslateMonth(cacheService.SelectedDay.Month);
		return $"{month} {cacheService.SelectedDay.Year}";
	}

	protected override DateTime FloorIntervall(DateTime date) {
		return DateTimeService.FloorMonth(date);
	}

	protected override DateTime CeilIntervall(DateTime date) {
		return DateTimeService.CeilMonth(date);
	}

	protected override void PreviousIntervallClick() {
		cacheService.SelectedDay = DateTimeService.FloorMonth(cacheService.SelectedDay.AddMonths(-1));
	}

	protected override void FollowingIntervallClick() {
		cacheService.SelectedDay = DateTimeService.FloorMonth(cacheService.SelectedDay.AddMonths(1));
	}

	protected override void OnIntervallChanged(IntervallChangedEventArgs args) {
		base.OnIntervallChanged(args);
		YAxisSegmentCount = DateTimeService.WeeksInMonth(cacheService.SelectedDay);
	}

	public override async Task<List<Timespan.Types.Models.Task>> GetTasksAsync() {
		return dbService != null ? await dbService.QueryTasksOfMonthAtDateAsync(DateTimeService.FloorMonth(cacheService.SelectedDay)) : [];
	}
}
