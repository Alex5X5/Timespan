namespace Hourglass.GUI.ViewModels.Pages;

using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;

using Hourglass.Database.Models;
using Hourglass.Database.Services.Interfaces;
using Hourglass.GUI.Services;
using Hourglass.Util;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static Hourglass.GUI.ViewModels.MainViewModel;

public partial class TimerPageViewModel : PageViewModelBase, INotifyPropertyChanged {

    private IHourglassDbService dbService;
	private CacheService cacheService;
    private ViewModelFactory<MainViewModel> pageFactory;
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

    public override string Title => TranslatorService.Singleton["Views.Pages.Timer.Title"] ?? "Timer";

    public bool IsStartButtonEnabled { get => cacheService?.RunningTask == null; }
    public bool IsStopButtonEnabled { get => cacheService?.RunningTask != null; }
    public bool IsRestartButtonEnabled { get => cacheService?.RunningTask != null; }

    public Project SelectedProject { get; set; }
    public List<Project> AvailableProjects { get; set; }


    public new event PropertyChangedEventHandler? PropertyChanged;


	public TimerPageViewModel() : this(null, null) {
    }

	public TimerPageViewModel(IHourglassDbService dbService, CacheService cacheService) : base() {
		this.dbService = dbService;
		this.cacheService = cacheService;
		if(cacheService!=null)
			cacheService.OnRunningTaksChanged +=
				task => AllBindingPropertiesChanged();
        ButtonActions = new ObservableCollection<TabButtonAction>() {
            new TabButtonAction("start", ReactiveCommand.Create(()=>SetTabButtonSeleted(0)), true),
            new TabButtonAction("hallo", ReactiveCommand.Create(()=>SetTabButtonSeleted(1))),
            new TabButtonAction("mehh", ReactiveCommand.Create(()=>SetTabButtonSeleted(2))),
            new TabButtonAction("Stop", ReactiveCommand.Create(()=>SetTabButtonSeleted(3)))
        };
        _timer = new DispatcherTimer {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += async (s, e) => {
            try {
                cacheService!.RunningTask!.FinishDateTime = DateTime.Now;
                await dbService.UpdateTaskAsync(cacheService.RunningTask);
                FinishTextboxText = DateTimeService.ToDayAndMonthAndTimeString(cacheService.RunningTask.FinishDateTime);
            } catch (Exception ex) {
                StartTextboxText = $"Error: {ex.Message}";
            }
        };
    }

    private void SetTabButtonSeleted(int index) {
        foreach (var action in ButtonActions)
            action.Selected = action == ButtonActions[index];
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

	protected virtual void OnPropertyChanged(string propertyName) {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	[RelayCommand]
	private async System.Threading.Tasks.Task StartTask() {
		Console.WriteLine("start task button click!");
		if(dbService!=null)
			cacheService.RunningTask = await dbService.StartNewTaskAsnc(
				DescriptionTextboxText,
                new Color(255, 79, 79, 79),
				null,
				new Worker { name = "new user" },
				null
			);
        StartTextboxText = DateTimeService.ToDayAndMonthAndTimeString(cacheService.RunningTask!.StartDateTime);
		AllBindingPropertiesChanged();
		_timer.Start();
	}

	[RelayCommand]
	private async System.Threading.Tasks.Task StopTask() {
		Console.WriteLine("stop task button click!");
		_timer.Stop();
		if(dbService!=null)
            cacheService.RunningTask = await dbService.FinishCurrentTaskAsync(
				cacheService.RunningTask?.start ?? DateTimeService.ToSeconds(DateTime.Now),
				DateTimeService.ToSeconds(DateTime.Now),
				DescriptionTextboxText,
				SelectedProject,
				null
			);
        DescriptionTextboxText = "";
        StartTextboxText = "";
        FinishTextboxText = "";
        AllBindingPropertiesChanged();
    }

	[RelayCommand]
	private void RestartTask() {
		Console.WriteLine("restart task button click! (not yet implemented)");
		AllBindingPropertiesChanged();
	}

	public void OnLoad() {
		Console.WriteLine("loading Timer Page");
		cacheService.RunningTask = dbService.QueryCurrentTaskAsync().Result;
        if (cacheService.RunningTask?.running ?? false) {
            _timer.Start();
            StartTextboxText = DateTimeService.ToDayAndMonthAndTimeString(cacheService.RunningTask.StartDateTime);
            FinishTextboxText = DateTimeService.ToDayAndMonthAndTimeString(cacheService.RunningTask.FinishDateTime);
        }
        AllBindingPropertiesChanged();
	}

    public void OnUnload() {
        Console.WriteLine("unloading Timer Page");
        _timer.Stop();
    }
}