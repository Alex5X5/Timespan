namespace Timespan.GUI.Controls;

using Avalonia.Interactivity;
using Avalonia.Media;

using CommunityToolkit.Mvvm.Input;

using Timespan.GUI.Generators.Attributes;
using Timespan.GUI.Helpers;
using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

public partial class TaskDetailsControl : UserControl {

	#region styled properties

	[BasicStyledProperty<TaskDetailsControl>]
	private Types.ObservableTask selectedTask;

	[BasicStyledProperty<TaskDetailsControl>]
	private IRelayCommand closeCommand;

	[BasicStyledProperty<TaskDetailsControl>]
	private IRelayCommand<Timespan.Types.Models.Task> saveCommand;

	[BasicStyledProperty<TaskDetailsControl>]
	private IRelayCommand<Timespan.Types.Models.Task> continiueCommand;

	[BasicStyledProperty<TaskDetailsControl>]
	private IRelayCommand<Timespan.Types.Models.Task> stopCommand;

	[BasicStyledProperty<TaskDetailsControl>]
	private IRelayCommand<Timespan.Types.Models.Task> restartCommand;

	[BasicStyledProperty<TaskDetailsControl>]
	private IRelayCommand<Timespan.Types.Models.Task> deleteCommand;

	[BasicDirectProperty<TaskDetailsControl>]
	private string title = "A Title";

	[BasicDirectProperty<TaskDetailsControl>]
	private string description = "A Description";

	[BasicDirectProperty<TaskDetailsControl>]
	private string dateString = "A Date String";

	[BasicDirectProperty<TaskDetailsControl>]
	private string timeString = "A Time String";

	[BasicDirectProperty<TaskDetailsControl>]
	private string startTextboxText = "A Start Text";

	[BasicDirectProperty<TaskDetailsControl>]
	private string finishTextboxText = "A Finish Text";

	[BasicDirectProperty<TaskDetailsControl>]
	private Color selectedColor = Color.FromArgb(255, 70, 70, 70);

	[BasicDirectProperty<TaskDetailsControl>]
	private bool showReadonlyTaskPanel = true;

	[BasicDirectProperty<TaskDetailsControl>]
	private bool showEditTaskPanel = false;

	[BasicDirectProperty<TaskDetailsControl>]
	private bool taskRunning = false;

	[BasicDirectProperty<TaskDetailsControl>]
	private bool taskNotRunning = true;

	#endregion

	public TaskDetailsControl() {
		InitializeComponent();

		AddHandler(LoadedEvent, OnLoad);
		AddHandler(UnloadedEvent, OnUnload);
	}

	public void EditButtonClick(object sender, RoutedEventArgs e) {
		SetEdit();
		InvalidateVisual();
	}

	public void SaveButtonClick(object sender, RoutedEventArgs e) {
		SetReadonly();
		InvalidateVisual();
		Timespan.Types.Models.Task task = BuildTaskFromState();
		if (SaveCommand.CanExecute(task))
			SaveCommand.Execute(task);
	}

	public void CloseButtonClick(object sender, RoutedEventArgs e) {
		if (ShowReadonlyTaskPanel == true) {
			if (CloseCommand.CanExecute(EventArgs.Empty))
				CloseCommand.Execute(EventArgs.Empty);
		} else {
			SetReadonly();
			InsertSelectedTaskData();
		}
	}

	public void DeleteButtonClick(object sender, RoutedEventArgs e) {
		SetReadonly();
		DeleteCommand.Execute(SelectedTask.Value);
	}

	public void ContiniueButtonClick(object sender, RoutedEventArgs e) {
		SetReadonly();
		Timespan.Types.Models.Task task = BuildTaskFromState();
		if (ContiniueCommand.CanExecute(task))
			ContiniueCommand.Execute(task);
	}

	public void StopButtonClick(object sender, RoutedEventArgs e) {
		Timespan.Types.Models.Task task = BuildTaskFromState();
		if (StopCommand.CanExecute(task))
			StopCommand.Execute(task);
	}

	public void RestartButtonClick(object sender, RoutedEventArgs e) {
		Timespan.Types.Models.Task task = BuildTaskFromState();
		if (RestartCommand.CanExecute(task))
			RestartCommand.Execute(task);
	}

	private void OnLoad(object? sender, RoutedEventArgs args) {
		GlobalEventService.Subscribe<ShowTaksEventArgs>(OnShowTask);
		InsertSelectedTaskData();
		InvalidateVisual();
	}

	private void OnUnload(object? sender, RoutedEventArgs args) {
		GlobalEventService.UnSubscribe<ShowTaksEventArgs>(OnShowTask);
	}

	[RelayCommand]
	private void OnColorSelected(Color color) {
		SelectedColor = color;
	}

	private void OnShowTask(ShowTaksEventArgs args) {
		SetReadonly();
		InsertSelectedTaskData();
	}

	private Timespan.Types.Models.Task BuildTaskFromState() {
		var start = DateTimeService.InterpretDayAndTimeString(StartTextboxText) ?? SelectedTask.StartDateTime;
		var finish = DateTimeService.InterpretDayAndTimeString(FinishTextboxText) ?? SelectedTask.FinishDateTime;
		return new() {
			Id = SelectedTask.Id,
			description = Description,
			start = DateTimeService.ToSeconds(start),
			finish = DateTimeService.ToSeconds(finish),
			running = SelectedTask.Running,
			blocksTime = Timespan.Types.Models.BlockedTimeIntervallType.None,
			DisplayColor = SelectedColor
		};
	}

	private void InsertSelectedTaskData() {
		if (SelectedTask == null)
			return;
		TaskRunning = SelectedTask.Running;
		TaskNotRunning = !SelectedTask.Running;
		Description = SelectedTask.Description;
		Title = TaskHelper.GetTitleString(SelectedTask.Description);
		DateString = GetDateString(SelectedTask.StartDateTime);
		TimeString = GetTimeString(SelectedTask.StartDateTime, SelectedTask.FinishDateTime);
		StartTextboxText = DateTimeService.ToDayAndMonthAndTimeString(SelectedTask.StartDateTime);
		FinishTextboxText = DateTimeService.ToDayAndMonthAndTimeString(SelectedTask.FinishDateTime);
		SelectedColor = SelectedTask.DisplayColor;
	}

	private static string GetDateString(DateTime date) {
		string day = TranslatorService.Singleton.TranslateDayShort(date.DayOfWeek);
		string month = TranslatorService.Singleton.TranslateMonthShort(date.Month);
		return $"{day}. {date.Day}. {month} {date.Year}";
	}

	private static string GetTimeString(DateTime start, DateTime stop) {
		string start_ = DateTimeService.ToHourMinuteString(start);
		string stop_ = DateTimeService.ToHourMinuteString(stop);
		return $"{start_} - {stop_}";
	}

	private void SetReadonly() {
		ShowReadonlyTaskPanel = true;
		ShowEditTaskPanel = false;
	}

	private void SetEdit() {
		ShowReadonlyTaskPanel = false;
		ShowEditTaskPanel = true;
	}
}