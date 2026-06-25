namespace Timespan.GUI.Controls;

using Avalonia.Interactivity;
using Avalonia.Media;

using CommunityToolkit.Mvvm.Input;

using Timespan.GUI.Generators.Attributes;
using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

public partial class TaskDetailsControl : UserControl {

	public const int MAX_TASK_DESCRIPTION_CHARS = 30;

	#region styled properties

	public static readonly StyledProperty<Types.ObservableTask> SelectedTaskProperty =
		AvaloniaProperty.Register<TaskDetailsControl, Types.ObservableTask>(nameof(SelectedTask), new Types.ObservableTask(null));

	public static readonly StyledProperty<IRelayCommand> CloseCommandProperty =
		AvaloniaProperty.Register<TaskDetailsControl, IRelayCommand>(nameof(CloseCommand), new RelayCommand(() => { }));

	public static readonly StyledProperty<IRelayCommand<Timespan.Types.Models.Task>> SaveCommandProperty =
		AvaloniaProperty.Register<TaskDetailsControl, IRelayCommand<Timespan.Types.Models.Task>>(nameof(SaveCommand), new RelayCommand<Timespan.Types.Models.Task>(task => { }));

	public static readonly StyledProperty<IRelayCommand> DeleteCommandProperty =
		AvaloniaProperty.Register<TaskDetailsControl, IRelayCommand>(nameof(DeleteCommand), new RelayCommand(() => { }));
	
	public Types.ObservableTask SelectedTask {
		get => GetValue(SelectedTaskProperty);
		set => SetValue(SelectedTaskProperty, value);
	}

	public IRelayCommand CloseCommand {
		get => GetValue(CloseCommandProperty);
		set => SetValue(CloseCommandProperty, value);
	}

	public IRelayCommand DeleteCommand {
		get => GetValue(DeleteCommandProperty);
		set => SetValue(DeleteCommandProperty, value);
	}

	public IRelayCommand<Timespan.Types.Models.Task> SaveCommand {
		get => GetValue(SaveCommandProperty);
		set => SetValue(SaveCommandProperty, value);
	}

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

	#endregion

	public TaskDetailsControl() {
		InitializeComponent();
		AddHandler(LoadedEvent, OnLoad);
		AddHandler(UnloadedEvent, OnUnload);
	}

	public void EditButtonClick(object sender, RoutedEventArgs e) {
		ShowReadonlyTaskPanel = false;
		ShowEditTaskPanel = true;
		InvalidateVisual();
	}

	public void SaveButtonClick(object sender, RoutedEventArgs e) {
		ShowReadonlyTaskPanel = true;
		ShowEditTaskPanel = false;
		InvalidateVisual();
		var start = DateTimeService.InterpretDayAndTimeString(StartTextboxText) ?? SelectedTask.StartDateTime;
		var finish = DateTimeService.InterpretDayAndTimeString(FinishTextboxText) ?? SelectedTask.FinishDateTime;
		Timespan.Types.Models.Task task = new() {
			Id = SelectedTask.Id,
			description = Description,
			start = DateTimeService.ToSeconds(start),
			finish = DateTimeService.ToSeconds(finish),
			running = SelectedTask.Running,
			blocksTime = Timespan.Types.Models.BlockedTimeIntervallType.None,
			DisplayColor = SelectedColor
		};
		if (SaveCommand.CanExecute(task))
			SaveCommand.Execute(task);
	}

	public void CloseButtonClick(object sender, RoutedEventArgs e) {
		if (ShowReadonlyTaskPanel == true) {
			if (CloseCommand.CanExecute(EventArgs.Empty))
				CloseCommand.Execute(EventArgs.Empty);
		} else {
			ShowReadonlyTaskPanel = true;
			ShowEditTaskPanel = false;
			InsertSelectedTaskData();
		}
	}

	public void DeleteButtonClick(object sender, RoutedEventArgs e) {
		ShowReadonlyTaskPanel = true;
		ShowEditTaskPanel = false;
		if (DeleteCommand.CanExecute(EventArgs.Empty))
			DeleteCommand.Execute(EventArgs.Empty);
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
		ShowReadonlyTaskPanel = true;
		ShowEditTaskPanel = false;
		InsertSelectedTaskData();
	}

	private void InsertSelectedTaskData() {
		if (SelectedTask == null)
			return;
		Description = SelectedTask.Description;
		Title = GetTitleString(SelectedTask.Description);
		DateString = GetDateString(SelectedTask.StartDateTime);
		TimeString = GetTimeString(SelectedTask.StartDateTime, SelectedTask.FinishDateTime);
		StartTextboxText = DateTimeService.ToDayAndMonthAndTimeString(SelectedTask.StartDateTime);
		FinishTextboxText = DateTimeService.ToDayAndMonthAndTimeString(SelectedTask.FinishDateTime);
		SelectedColor = SelectedTask.DisplayColor;
	}

	private static string GetTitleString(string description) {
		if (description.Length <= MAX_TASK_DESCRIPTION_CHARS)
			return description;
		List<char> res = [];
		List<char> word = [];
		for (int i = 0; i < MAX_TASK_DESCRIPTION_CHARS && i < description.Length; i++) {
			char current = description[i];
			if (current == ' ') {
				if (res.Count + 1 + word.Count <= MAX_TASK_DESCRIPTION_CHARS) {
					res.AddRange(word);
					res.Add(current);
					word = [];
				}
				continue;
			}
			word.Add(current);
		}
		return new(res.ToArray());
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
		if (change.Property == SelectedTaskProperty) {
			Console.WriteLine("[TaskDetailsControl]:Task changed");
		}
		base.OnPropertyChanged(change);
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
}