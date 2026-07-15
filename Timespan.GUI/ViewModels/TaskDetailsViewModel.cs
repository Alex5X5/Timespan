namespace Timespan.GUI.ViewModels;

using Avalonia.Interactivity;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Helpers;
using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

public partial class TaskDetailsViewModel : ViewModelBase {

	private ITimespanDbService dbService;
	private GuiStateService stateService;

	#region Observable Properties

	[ObservableProperty]
	private string title = "A Title";

	[ObservableProperty]
	private string description = "A Description";

	[ObservableProperty]
	private string dateString = "A Date String";

	[ObservableProperty]
	private string timeString = "A Time String";

	[ObservableProperty]
	private string startTextboxText = "A Start Text";

	[ObservableProperty]
	private string finishTextboxText = "A Finish Text";

	[ObservableProperty]
	private bool showReadonlyTaskPanel = true;

	[ObservableProperty]
	private bool showEditTaskPanel = false;

	[ObservableProperty]
	public partial bool ShowStopButton { get; set; } = false;

	[ObservableProperty]
	private bool showRestartButton = false;

	[ObservableProperty]
	private bool showContiniueButton = false;

	public Color SelectedColor {
		set {
			OnPropertyChanging(nameof(SelectedColor));
			stateService.SelectedColor = value;
			OnPropertyChanged(nameof(SelectedColor));
		}
		get => stateService.SelectedColor;
	}

	public Timespan.Types.Models.Task SelectedTask {
		set {
			OnPropertyChanging(nameof(SelectedTask));
			stateService.SelectedTask = value;
			OnPropertyChanged(nameof(SelectedTask));
		}
		get => stateService.SelectedTask;
	}

	#endregion

	public TaskDetailsViewModel(ITimespanDbService dbService, GuiStateService stateService) {
		this.dbService = dbService;
		this.stateService = stateService;
	}

	#region Button Callbacks

	[RelayCommand]
	public async Task DeleteTask() {
		await dbService.DeleteTaskAsync(SelectedTask);
		await RaiseTasksChangedAsync();
	}

	[RelayCommand]
	private async Task ContiniueTask() {
		await dbService.ContiniueTaskAsync(SelectedTask);
		await RaiseTasksChangedAsync();
	}

	[RelayCommand]
	private async Task StopTaskAsync() {
		if (dbService != null) {
			SelectedTask.running = false;
			await dbService.UpdateTaskAsync(SelectedTask);
			await RaiseTasksChangedAsync();
		}
	}

	private async Task StartTask() {
		if (dbService != null) {
			var task_ = await dbService.StartNewTaskAsnc(
				SelectedTask.description,
				SelectedTask.DisplayColor,
				SelectedTask.project,
				SelectedTask.owner,
				SelectedTask.ticket
			);
			stateService.RunningTask = task_;
			await RaiseTasksChangedAsync();
		}
	}

	[RelayCommand]
	private async Task RestartTask() {
		await StopTaskAsync();
		await StartTask();
		await RaiseTasksChangedAsync();
	}

	[RelayCommand]
	private async Task SaveTask() {
		SetReadonly();
		var task = GetTaskFromState();
		await dbService.UpdateTaskAsync(task);
		await RaiseTasksChangedAsync();
		await RefetchAndShowAsync();
	}

	[RelayCommand]
	private void EditTask() {
		SetEdit();
	}

	[RelayCommand]
	private async Task CloseTask() {
		if (ShowReadonlyTaskPanel == true) {
			GlobalEventService.Raise(new ShowTaksEventArgs(null));
		} else {
			SetReadonly();
			SetStateFromTask();
		}
	}

	#endregion

	#region Events

	[RelayCommand]
	private void OnLoad() {
		GlobalEventService.Subscribe<ShowTaksEventArgs>(OnShowTask);
		GlobalEventService.Subscribe<TasksChangedEventArgs>(OnTasksChanged);
	}

	[RelayCommand]
	private void OnUnload() {
		GlobalEventService.UnSubscribe<ShowTaksEventArgs>(OnShowTask);
		GlobalEventService.UnSubscribe<TasksChangedEventArgs>(OnTasksChanged);
	}

	[RelayCommand]
	private void OnColorSelected(Color color) {
		SelectedColor = color;
	}

	private async void OnShowTask(ShowTaksEventArgs args) {
		SetReadonly();
		await SetStateFromTaskAsync();
	}

	private async void OnTasksChanged(TasksChangedEventArgs args) {
		await RefetchAndShowAsync();
	}

	#endregion

	#region display logic

	private void SetReadonly() {
		ShowReadonlyTaskPanel = true;
		ShowEditTaskPanel = false;
	}

	private void SetEdit() {
		ShowReadonlyTaskPanel = false;
		ShowEditTaskPanel = true;
		ShowContiniueButton = false;
		ShowRestartButton = false;
		ShowStopButton = false;
	}

	private Timespan.Types.Models.Task GetTaskFromState() {
		var start = DateTimeService.InterpretDayAndTimeString(StartTextboxText) ?? SelectedTask.StartDateTime;
		var finish = DateTimeService.InterpretDayAndTimeString(FinishTextboxText) ?? SelectedTask.FinishDateTime;
		return new() {
			Id = SelectedTask.Id,
			description = Description,
			start = DateTimeService.ToSeconds(start),
			finish = DateTimeService.ToSeconds(finish),
			running = SelectedTask.running,
			blocksTime = Timespan.Types.Models.BlockedTimeIntervallType.None,
			DisplayColor = SelectedColor
		};
	}

	private static string GetDateString(DateTime date) {
		string day = TranslatorService.Singleton.TranslateDayShort(date.DayOfWeek);
		string month = TranslatorService.Singleton.TranslateMonthShort(date.Month);
		return $"{day}. {date.Day}. {month} {date.Year}";
	}

	private static string GetTimeString(DateTime start, DateTime stop) {
		string start_ = DateTimeService.ToHourMinuteStringSinceMidnight(start);
		string stop_ = DateTimeService.ToHourMinuteStringSinceMidnight(stop);
		return $"{start_} - {stop_}";
	}

	#endregion

	#region data updating

	private static async Task RaiseTasksChangedAsync() {
		await Dispatcher.UIThread.InvokeAsync(
			()=> GlobalEventService.Raise<TasksChangedEventArgs>());
	}

	private async Task RefetchAndShowAsync() {
		if (SelectedTask == null)
			return;
		var refetchedTask = await dbService.QueryTasksByIdAsync(SelectedTask.Id);
		await Dispatcher.UIThread.InvokeAsync(
			() => {
				SelectedTask = refetchedTask ?? SelectedTask;
			});
		await SetStateFromTaskAsync();
	}

	private async Task SetStateFromTaskAsync() {
		await Dispatcher.UIThread.InvokeAsync(SetStateFromTask);
	}

	private void SetStateFromTask() {
		if (SelectedTask == null)
			return;
		Description = SelectedTask.description;
		Title = TaskHelper.GetTitleString(SelectedTask.description);
		DateString = GetDateString(SelectedTask.StartDateTime);
		TimeString = GetTimeString(SelectedTask.StartDateTime, SelectedTask.FinishDateTime);
		StartTextboxText = DateTimeService.ToDayAndMonthAndTimeString(SelectedTask.StartDateTime);
		FinishTextboxText = DateTimeService.ToDayAndMonthAndTimeString(SelectedTask.FinishDateTime);
		SelectedColor = SelectedTask.DisplayColor;
		UpdateTaskRunningAsync();
	}

	private async Task UpdateTaskRunningAsync() {
		var task = (await dbService.QueryCurrentTaskAsync());
		var anyRunning = task != null;
		var selectedRunning = task?.Id == SelectedTask?.Id;
		await Dispatcher.UIThread.InvokeAsync(
			() => {
				if (ShowEditTaskPanel) {
					ShowRestartButton = false;
					ShowStopButton = false;
					ShowContiniueButton = false;
				} else {
					ShowRestartButton = !anyRunning;
					ShowStopButton = selectedRunning;
					ShowContiniueButton = !anyRunning;
				}
			});
	}

	#endregion
}
