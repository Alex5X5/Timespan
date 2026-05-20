namespace Timespan.GUI.ViewModels;

using Avalonia.Media;
using Avalonia.Threading;

using CommunityToolkit.Mvvm.Input;

using System.ComponentModel;

using Timespan.Database.Services.Interfaces;
using Timespan.Types.Models;
using Timespan.Util.Services;


public partial class TimerViewModel : ViewModelBase, IMainViewChild, INotifyPropertyChanged {

	private IHourglassDbService dbService;
	private Services.CacheService cacheService;
	private MainViewModel controller;

	private DispatcherTimer _timer;

	private string FallbackTaskDescription = "";

	public string DescriptionTextboxText {
		set {
			if (cacheService?.RunningTask != null)
				cacheService.RunningTask.description = value;
			else
				FallbackTaskDescription = value;
			OnPropertyChanged(nameof(DescriptionTextboxText));
		}
		get => cacheService?.RunningTask?.description ?? FallbackTaskDescription;
	}
	public string ProjectTextboxText {
		set {
			if (cacheService?.RunningTask != null)
				cacheService.RunningTask.description = value;
			OnPropertyChanged(nameof(ProjectTextboxText));
		}
		get => cacheService?.RunningTask?.project?.Name ?? "";
	}
	public string TicketTextboxText {
		get => cacheService?.RunningTask?.ticket?.name ?? "";
	}


	private string startTextboxText = "";
	public string StartTextboxText {
		set {
			startTextboxText = value;
			OnPropertyChanged(nameof(FinishTextboxText));
			DateTime? start = DateTimeService.InterpretDayAndTimeString(value);
			if (start == null)
				return;
			if (cacheService?.RunningTask == null)
				return;
			cacheService.RunningTask.StartDateTime = start ?? cacheService.RunningTask.StartDateTime;
		}
		get => startTextboxText;
	}

	private string finishTextboxText = "";
	public string FinishTextboxText {
		set {
			finishTextboxText = value;
			OnPropertyChanged(nameof(FinishTextboxText));
			DateTime? finish = DateTimeService.InterpretDayAndTimeString(value);
			if (finish == null)
				return;
			if (cacheService?.RunningTask == null)
				return;
			cacheService.RunningTask.FinishDateTime = finish ?? cacheService.RunningTask.FinishDateTime;
		}
		get => finishTextboxText;
	}

	public bool IsStartButtonEnabled { get => cacheService?.RunningTask == null; }
    public bool IsStopButtonEnabled { get => cacheService?.RunningTask != null; }
    public bool IsRestartButtonEnabled { get => cacheService?.RunningTask != null; }


	public TimerViewModel(IHourglassDbService dbService, Services.CacheService cacheService) : base() {
		this.dbService = dbService;
		this.cacheService = cacheService;
		if (cacheService != null)
			cacheService.OnRunningTaksChanged +=
				task => AllBindingPropertiesChanged();
		_timer = new DispatcherTimer {
			Interval = TimeSpan.FromSeconds(1)
		};
		_timer.Tick += async (s, e) => {
			try {
				cacheService!.RunningTask!.FinishDateTime = DateTime.Now;
				await dbService.UpdateTaskAsync(cacheService.RunningTask);
				AllBindingPropertiesChanged();
				//FinishTextboxText = DateTimeService.ToDayAndMonthAndTimeString(cacheService.RunningTask.FinishDateTime);
			} catch (Exception ex) {
				StartTextboxText = $"Error: {ex.Message}";
			}
		};
	}

	[RelayCommand]
	private async System.Threading.Tasks.Task StartTask() {
		Console.WriteLine("model start task button event!");
		if (dbService != null)
			cacheService.RunningTask = await dbService.StartNewTaskAsnc(
				DescriptionTextboxText,
				new Color(255, 79, 79, 79),
				null,
				new Worker { name = "new user" },
				null
			);
		StartTextboxText = DateTimeService.ToDayAndMonthAndTimeString(cacheService.RunningTask!.StartDateTime);
		FinishTextboxText = DateTimeService.ToDayAndMonthAndTimeString(cacheService.RunningTask.FinishDateTime);
		_timer.Start();
	}

	[RelayCommand]
	private async System.Threading.Tasks.Task StopTask() {
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
	}

	private void AllBindingPropertiesChanged() {
		OnPropertyChanged(nameof(DescriptionTextboxText));
		OnPropertyChanged(nameof(StartTextboxText));
		OnPropertyChanged(nameof(FinishTextboxText));
		OnPropertyChanged(nameof(TicketTextboxText));
		OnPropertyChanged(nameof(ProjectTextboxText));
		OnPropertyChanged(nameof(IsStartButtonEnabled));
		OnPropertyChanged(nameof(IsStopButtonEnabled));
		OnPropertyChanged(nameof(IsRestartButtonEnabled));
	}

	public void OnLoad() {
		cacheService.RunningTask = dbService.QueryCurrentTaskAsync().Result;
		if (cacheService.RunningTask?.running ?? false) {
			_timer.Start();
			StartTextboxText = DateTimeService.ToDayAndMonthAndTimeString(cacheService.RunningTask.StartDateTime);
			FinishTextboxText = DateTimeService.ToDayAndMonthAndTimeString(cacheService.RunningTask.FinishDateTime);
		}
	}

	public void OnUnload() {
		_timer.Stop();
	}
}
