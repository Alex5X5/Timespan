namespace Timespan.GUI.Controls;

using Avalonia.Interactivity;

using CommunityToolkit.Mvvm.Input;

using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;

internal partial class TaskDetailsControl : UserControl {

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

	public static readonly DirectProperty<TaskDetailsControl, string> TitleProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, string>(
			nameof(Title),
			control => control.Title,
			(control, value) => control.Title = value,
			"",
			Avalonia.Data.BindingMode.TwoWay);
	
	public static readonly DirectProperty<TaskDetailsControl, string> DescriptionProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, string>(
			nameof(Description),
			control => control.Description,
			(control, value) => control.Description = value,
			"",
			Avalonia.Data.BindingMode.TwoWay);

	public static readonly DirectProperty<TaskDetailsControl, string> DateStringProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, string>(
			nameof(DateString),
			control => control.DateString,
			(control, value) => control.DateString = value,
			"",
			Avalonia.Data.BindingMode.TwoWay);

	public static readonly DirectProperty<TaskDetailsControl, string> TimeStringProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, string>(
			nameof(TimeString),
			control => control.TimeString,
			(control, value) => control.TimeString = value,
			"",
			Avalonia.Data.BindingMode.TwoWay);

	public static readonly DirectProperty<TaskDetailsControl, string> StartTextboxTextProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, string>(
			nameof(StartTextboxText),
			control => control.StartTextboxText,
			(control, value) => control.StartTextboxText = value,
			"",
			Avalonia.Data.BindingMode.TwoWay);

	public static readonly DirectProperty<TaskDetailsControl, string> FinishTextboxTextProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, string>(
			nameof(FinishTextboxText),
			control => control.FinishTextboxText,
			(control, value) => control.FinishTextboxText = value,
			"",
			Avalonia.Data.BindingMode.TwoWay);

	public static readonly DirectProperty<TaskDetailsControl, string> DescriptionTextboxTextProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, string>(
			nameof(DescriptionTextboxText),
			control => control.DescriptionTextboxText,
			(control, value) => control.DescriptionTextboxText = value,
			"",
			Avalonia.Data.BindingMode.TwoWay);

	public static readonly DirectProperty<TaskDetailsControl, bool> ShowReadonlyTaskPanelProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, bool>(
			nameof(ShowReadonlyTaskPanel),
			control => control.ShowReadonlyTaskPanel,
			(control, value) => control.ShowReadonlyTaskPanel = value,
			true,
			Avalonia.Data.BindingMode.TwoWay);

	public static readonly DirectProperty<TaskDetailsControl, bool> ShowEditTaskPanelProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, bool>(
			nameof(ShowEditTaskPanel),
			control => control.ShowEditTaskPanel,
			(control, value) => control.ShowEditTaskPanel = value,
			false,
			Avalonia.Data.BindingMode.TwoWay);

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

	public string Title {
		get => title;
		set => SetAndRaise(TitleProperty, ref title, value);
	}
	private string title = "A Title";

	public string Description {
		get => description;
		set => SetAndRaise(DescriptionProperty, ref description, value);
	}
	private string description = "A Description";

	public string DateString {
		get => dateString;
		set => SetAndRaise(DateStringProperty, ref dateString, value);
	}
	private string dateString = "A Date String";

	public string TimeString {
		get => timeString;
		set => SetAndRaise(TimeStringProperty, ref timeString, value);
	}
	private string timeString = "A Time String";

	public string StartTextboxText {
		get => timeString;
		set => SetAndRaise(StartTextboxTextProperty, ref timeString, value);
	}
	private string startTextboxText = "A Start Text";

	public string FinishTextboxText {
		get => timeString;
		set => SetAndRaise(FinishTextboxTextProperty, ref timeString, value);
	}
	private string finishTextboxText = "A Finish Text";

	public string DescriptionTextboxText {
		get => timeString;
		set => SetAndRaise(DescriptionTextboxTextProperty, ref timeString, value);
	}
	private string descriptionTextboxText = "A Description Text";

	public bool ShowReadonlyTaskPanel {
		get => showReadonlyTaskPanel;
		set => SetAndRaise(ShowReadonlyTaskPanelProperty, ref showReadonlyTaskPanel, value);
	}
	private bool showReadonlyTaskPanel = true;

	public bool ShowEditTaskPanel {
		get => showEditTaskPanel;
		set => SetAndRaise(ShowEditTaskPanelProperty, ref showEditTaskPanel, value);
	}
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
		var start = DateTimeService.InterpretDayAndTimeString(StartTextboxText);
		var finish = DateTimeService.InterpretDayAndTimeString(FinishTextboxText);
		Timespan.Types.Models.Task task = new();
		if (SaveCommand.CanExecute(EventArgs.Empty))
			SaveCommand.Execute(EventArgs.Empty);
		if (CloseCommand.CanExecute(EventArgs.Empty))
			CloseCommand.Execute(EventArgs.Empty);
	}

	public void CloseButtonClick(object sender, RoutedEventArgs e) {
		ShowReadonlyTaskPanel = true;
		ShowEditTaskPanel = false;
		if (CloseCommand.CanExecute(EventArgs.Empty))
			CloseCommand.Execute(EventArgs.Empty);
	}

	public void DeleteButtonClick(object sender, RoutedEventArgs e) {
		ShowReadonlyTaskPanel = true;
		ShowEditTaskPanel = false;
		if (DeleteCommand.CanExecute(EventArgs.Empty))
			DeleteCommand.Execute(EventArgs.Empty);
	}

	private void OnLoad(object? sender, RoutedEventArgs args) {
		GlobalEventService.Subscribe<ShowTaksEventArgs>(ShowTask);
		InvalidateVisual();
	}

	private void OnUnload(object? sender, RoutedEventArgs args) {
		GlobalEventService.UnSubscribe<ShowTaksEventArgs>(ShowTask);
	}

	private void ShowTask(ShowTaksEventArgs args) {
		ShowReadonlyTaskPanel = true;
		ShowEditTaskPanel = false;
		if (args.Task is Timespan.Types.Models.Task task) {
			Description = args.Task?.description ?? "";
			Title = GetTitleString(args.Task?.description ?? "");
			DateString = GetDateString(task.StartDateTime);
			TimeString = GetTimeString(task.StartDateTime, task.FinishDateTime);
		}
	}

	private static string GetTitleString(string description) {
		char[] dots = ['.', '.', '.'];
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
		//res.AddRange(dots);
		return new(res.ToArray());
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