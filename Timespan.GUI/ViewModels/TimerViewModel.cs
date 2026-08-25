namespace Timespan.GUI.ViewModels;

using Avalonia.Media;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.ComponentModel;
using System.Threading.Tasks;

using Timespan.Database.Services;
using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Services.Mapping;
using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;
using Timespan.Util.Services;


public partial class TimerViewModel : ViewModelBase, IMainViewChild, INotifyPropertyChanged {

	private ITimespanDbService dbService;
	private readonly GuiStateService stateService;

	private readonly DispatcherTimer _timer;
	
	[ObservableProperty]
	private string descriptionTextboxText = "";

	[ObservableProperty]
	private string projectTextboxText = "";

	[ObservableProperty]
	private string startTextboxText = "";

	[ObservableProperty]
	private string finishTextboxText = "";

	[ObservableProperty]
	private ObservableTask selectedTask;

	public bool IsStartButtonEnabled { get => stateService.RunningTask == null; }
	public bool IsStopButtonEnabled { get => stateService?.RunningTask != null; }

	public TimerViewModel() : this(new TimespanDbService(), new GuiStateService(new CacheService())) {
		
	}

	public TimerViewModel(ITimespanDbService dbService, GuiStateService stateService) : base() {
		this.dbService = dbService;
		this.stateService = stateService;
		_timer = new DispatcherTimer {
			Interval = TimeSpan.FromSeconds(1)
		};
		_timer.Tick += TimerTick;
	}

	private void UpdateDisplayTask() {
		if (stateService.RunningTask is not Timespan.Types.Models.Task running)
			return;
		StartTextboxText = DateTimeService.ToDayAndMonthAndTimeString(stateService.RunningTask.StartDateTime);
		FinishTextboxText = DateTimeService.ToDayAndMonthAndTimeString(stateService.RunningTask.FinishDateTime);
		DescriptionTextboxText = stateService.RunningTask.description;
		SelectedTask = TaskMapper.ToGuiType(running);
	}

	private async Task UpdateCacheTask() {
		if (stateService.RunningTask is not Timespan.Types.Models.Task running)
			return;
		running.StartDateTime = DateTimeService.InterpretDayAndTimeString(StartTextboxText) ?? running.StartDateTime;
		running.FinishDateTime = DateTimeService.InterpretDayAndTimeString(FinishTextboxText) ?? running.FinishDateTime;
		running.description = DescriptionTextboxText;
		stateService.RunningTask = running;
		await dbService.UpdateTaskAsync(running);
	}

	private async void TimerTick(object? sender, EventArgs args) {
		FinishTextboxText = DateTimeService.ToDayAndMonthAndTimeString(DateTime.Now);
        await UpdateCacheTask();
		UpdateDisplayTask();
	}

	partial void OnDescriptionTextboxTextChanged(string value) {
		if (stateService.RunningTask is not Timespan.Types.Models.Task running)
			return;
		running.description = DescriptionTextboxText;
	}

	[RelayCommand]
	private async Task StartTask() {
		Console.WriteLine("model start task button event!");
		if (dbService != null)
			stateService.RunningTask = await dbService.StartNewTaskAsnc(
				DescriptionTextboxText,
				stateService.SelectedColor,
				null,
				new Timespan.Types.Models.Worker { name = "new user" },
				null
			);
		UpdateDisplayTask();
		_timer.Start();
		UpdateButtons();
	}

	[RelayCommand]
	private async Task StopTask() {
		Console.WriteLine("model stop task button event!");
		_timer.Stop();
		if (dbService != null)
			if (await dbService.FinishCurrentTaskAsync(
				stateService.RunningTask?.start ?? DateTimeService.ToSeconds(DateTime.Now),
				DateTimeService.ToSeconds(DateTime.Now),
				DescriptionTextboxText,
				null,
				null))
				stateService.RunningTask = null;
		DescriptionTextboxText = "";
		StartTextboxText = "";
		FinishTextboxText = "";
		UpdateButtons();
	}

	private async void OnColorSelected(ColorSelectedEventArgs args) {
		if (SelectedTask != null) {
			SelectedTask.DisplayColor = stateService.SelectedColor;
			await dbService.UpdateTaskAsync(SelectedTask.Value);
		}
	}

	private void UpdateButtons() {
		OnPropertyChanged(nameof(IsStartButtonEnabled));
		OnPropertyChanged(nameof(IsStopButtonEnabled));
	}

	public void OnLoad() {
		GlobalEventService.Subscribe<ColorSelectedEventArgs>(OnColorSelected);
		stateService.RunningTask = dbService.QueryCurrentTaskAsync().Result;
		UpdateButtons();
		if (stateService.RunningTask?.running ?? false) {
			UpdateDisplayTask();
			_timer.Start();
		}
	}

	public void OnUnload() {
		GlobalEventService.UnSubscribe<ColorSelectedEventArgs>(OnColorSelected);
		_timer.Stop();
	}
}
