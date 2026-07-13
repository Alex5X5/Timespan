namespace Timespan.GUI.ViewModels;

using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.ComponentModel;
using System.Threading.Tasks;
using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services.Mapping;
using Timespan.GUI.Types;
using Timespan.Util.Services;


public partial class TimerViewModel : ViewModelBase, IMainViewChild, INotifyPropertyChanged {

	private ITimespanDbService dbService;
	private Services.CacheService cacheService;

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

	[ObservableProperty]
	private Color selectedColor;

	public bool IsStartButtonEnabled { get => cacheService?.RunningTask == null; }
	public bool IsStopButtonEnabled { get => cacheService?.RunningTask != null; }


	public TimerViewModel(ITimespanDbService dbService, Services.CacheService cacheService) : base() {
		this.dbService = dbService;
		this.cacheService = cacheService;
		_timer = new DispatcherTimer {
			Interval = TimeSpan.FromSeconds(1)
		};
		_timer.Tick += TimerTick;
	}

	private void UpdateDisplayTask() {
		if (cacheService.RunningTask is not Timespan.Types.Models.Task running)
			return;
		StartTextboxText = DateTimeService.ToDayAndMonthAndTimeString(cacheService.RunningTask.StartDateTime);
		FinishTextboxText = DateTimeService.ToDayAndMonthAndTimeString(cacheService.RunningTask.FinishDateTime);
		DescriptionTextboxText = cacheService.RunningTask.description;
		SelectedTask = TaskMapper.ToGuiType(running);
	}

	private async Task UpdateCacheTask() {
		if (cacheService.RunningTask is not Timespan.Types.Models.Task running)
			return;
		running.StartDateTime = DateTimeService.InterpretDayAndTimeString(StartTextboxText) ?? running.StartDateTime;
		running.FinishDateTime = DateTimeService.InterpretDayAndTimeString(FinishTextboxText) ?? running.FinishDateTime;
		running.description = DescriptionTextboxText;
		cacheService.RunningTask = running;
		await dbService.UpdateTaskAsync(running);
	}

	private async void TimerTick(object? sender, EventArgs args) {
		FinishTextboxText = DateTimeService.ToDayAndMonthAndTimeString(DateTime.Now);
        await UpdateCacheTask();
		UpdateDisplayTask();
	}

	partial void OnDescriptionTextboxTextChanged(string value) {
		if (cacheService.RunningTask is not Timespan.Types.Models.Task running)
			return;
		running.description = DescriptionTextboxText;
	}

	[RelayCommand]
	private async Task StartTask() {
		Console.WriteLine("model start task button event!");
		if (dbService != null)
			cacheService.RunningTask = await dbService.StartNewTaskAsnc(
				DescriptionTextboxText,
				SelectedColor,
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
			cacheService.RunningTask = await dbService.FinishCurrentTaskAsync(
				cacheService.RunningTask?.start ?? DateTimeService.ToSeconds(DateTime.Now),
				DateTimeService.ToSeconds(DateTime.Now),
				DescriptionTextboxText,
				null,
				null
			);
		DescriptionTextboxText = "";
		StartTextboxText = "";
		FinishTextboxText = "";
		UpdateButtons();
	}

	[RelayCommand]
	private async Task OnColorSelected(Avalonia.Media.Color color) {
		SelectedColor = color;
		if (SelectedTask != null) {
			SelectedTask.DisplayColor = color;
			await dbService.UpdateTaskAsync(SelectedTask.Value);
		}
	}

	private void UpdateButtons() {
		OnPropertyChanged(nameof(IsStartButtonEnabled));
		OnPropertyChanged(nameof(IsStopButtonEnabled));
	}

	public void OnLoad() {
		cacheService.RunningTask = dbService.QueryCurrentTaskAsync().Result;
		UpdateButtons();
		if (cacheService.RunningTask?.running ?? false) {
			UpdateDisplayTask();
			_timer.Start();
		}
	}

	public void OnUnload() {
		_timer.Stop();
	}
}
