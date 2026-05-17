namespace Hourglass.GUI.ViewModels.Pages;

using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;

using Timespan.Database.Models;
using Timespan.Database.Services.Interfaces;
using Hourglass.GUI.Services;
using Hourglass.Util;

using System.ComponentModel;

public partial class TaskDetailsPageViewModel : MainViewChildPageViewModel, INotifyPropertyChanged {

	private IHourglassDbService dbService;
    private CacheService cacheService;
	private ColorService colorService;
	private MainViewModel controller;

    public override string Title => TranslatorService.Singleton["Views.Pages.TaskDetails.Title"] ?? "Details";

    private readonly Task temporaryTask;
	private readonly Task selectedTask;

	public string DescriptionTextboxText {
        set {
            temporaryTask.description = value;
			DidChange = true;
            OnPropertyChanged(nameof(DescriptionTextboxText));
        }
        get => temporaryTask.description ?? "";
    }

	private string startTextboxText = "";
    public string StartTextboxText {
        set {
			startTextboxText = value;
            OnPropertyChanged(nameof(StartTextboxText));
			DateTime? start = DateTimeService.InterpretDayAndTimeString(value);
			if (start == null)
				return;
			temporaryTask.StartDateTime = start ?? temporaryTask.StartDateTime;
            DidChange = true;
        }
		get => startTextboxText;
    }

    private string finishTextboxText = "";
    public string FinishTextboxText {
        set {
            finishTextboxText = value;
            OnPropertyChanged(nameof(FinishTextboxText));
            DateTime? start = DateTimeService.InterpretDayAndTimeString(value);
            if (start == null)
                return;
            temporaryTask.FinishDateTime = start ?? temporaryTask.FinishDateTime;
            DidChange = true;
        }
        get => finishTextboxText;
    }

	private bool didChange = false;
	private bool DidChange { 
		set {
			if (value != didChange) {
				didChange = value;
				AllBindingPropertiesChanged();
			}			
		}
		get => didChange;
	}

    public bool IsContiniueButtonEnabled => cacheService.RunningTask == null;
	public bool IsStartNewButtonEnabled => cacheService.RunningTask == null;
    public bool IsSaveButtonEnabled => DidChange;
    public bool IsStopButtonEnabled => cacheService.SelectedTask?.running ?? false;
    public bool IsDeleteButtonEnabled => true;

	public new event PropertyChangedEventHandler? PropertyChanged;

	public TaskDetailsPageViewModel() : this(null, null, null, null) {
	}

	public TaskDetailsPageViewModel(IHourglassDbService dbService, MainViewModel pageController, CacheService cacheService, ColorService colorService) : base() {
		this.dbService = dbService;
		this.controller = pageController;
		this.cacheService = cacheService;
		this.colorService = colorService;
        temporaryTask = cacheService.SelectedTask?.Clone() ?? new Task();
    }

    private void AllBindingPropertiesChanged() {
        OnPropertyChanged(nameof(DescriptionTextboxText));
        OnPropertyChanged(nameof(StartTextboxText));
        OnPropertyChanged(nameof(FinishTextboxText));
		OnPropertyChanged(nameof(IsContiniueButtonEnabled));
		OnPropertyChanged(nameof(IsSaveButtonEnabled));
        OnPropertyChanged(nameof(IsStartNewButtonEnabled));
		OnPropertyChanged(nameof(IsStopButtonEnabled));
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
                cacheService.RunningTask?.DisplayColor ?? new Color(255, 79, 79, 79),
				null,
				new Worker { name = "new user" },
				null
			);
		AllBindingPropertiesChanged();
        controller.GoBack();
    }

	[RelayCommand]
	private async System.Threading.Tasks.Task StopTask() {
		Console.WriteLine("stop task button click!");
		if(dbService!=null)
			if(temporaryTask?.running ?? false)
				cacheService.RunningTask = await dbService.FinishCurrentTaskAsync(
                    temporaryTask.start,
					DateTimeService.ToSeconds(DateTime.Now),
					DescriptionTextboxText,
					null,
					null
				);
        controller.GoBack();
    }

	[RelayCommand]
	private async System.Threading.Tasks.Task RestartTask() {
		Console.WriteLine("start new button click! (not yet implemented)");
        await StopTask();
		await StartTask();
		cacheService.RunningTask!.description = temporaryTask!.description;
		cacheService.RunningTask!.displayColorRed = temporaryTask!.displayColorRed;
		cacheService.RunningTask!.displayColorGreen= temporaryTask!.displayColorGreen;
		cacheService.RunningTask!.displayColorBlue= temporaryTask!.displayColorBlue;
        await dbService.UpdateTaskAsync(cacheService.RunningTask);
		controller.GoBack();
    }

	[RelayCommand]
	private void DeleteTask() {
		Console.WriteLine("delete task button click!");
		if (temporaryTask == null)
			return;
		dbService.DeleteTaskAsync(temporaryTask);
		controller.GoBack();
	}

	[RelayCommand]
	private async System.Threading.Tasks.Task ContiniueTask() {
		Console.WriteLine("continiue task button click!");
		if(cacheService.SelectedTask == null)
			return;
		cacheService.RunningTask = await dbService.ContiniueTaskAsync(cacheService.SelectedTask);
		controller.GoBack();
	}

	[RelayCommand]
	private void ApplyChanges() {
		if (temporaryTask == null)
			return;
		dbService.UpdateTaskAsync(temporaryTask);
        controller.GoBack();
    }

	[RelayCommand]
	private void Cancel() {
		controller.GoBack();
	}

	public void OnLoad() {
		Console.WriteLine("loading Task Details Page Model");
        StartTextboxText = DateTimeService.ToDayAndMonthAndTimeString(temporaryTask.StartDateTime);
		FinishTextboxText = DateTimeService.ToDayAndMonthAndTimeString(temporaryTask.FinishDateTime);
		didChange = false;
		AllBindingPropertiesChanged();
    }

	[RelayCommand]
	public void Color1Button_Click() {
		AnyColorButtonClick(colorService.TASK_BACKGROUND_YELLOW);
	}

	[RelayCommand]
	public void Color2Button_Click() {
        AnyColorButtonClick(colorService.TASK_BACKGROUND_ORANGE);
    }

	[RelayCommand]
	public void Color3Button_Click() {
        AnyColorButtonClick(colorService.TASK_BACKGROUND_RED);
	}

	[RelayCommand]
	public void Color4Button_Click() {
        AnyColorButtonClick(colorService.TASK_BACKGROUND_LIGTH_BLUE);
	}

	[RelayCommand]
	public void Color5Button_Click() {
        AnyColorButtonClick(colorService.TASK_BACKGROUND_DARK_BLUE);
	}

	[RelayCommand]
	public void Color6Button_Click() {
        AnyColorButtonClick(colorService.TASK_BACKGROUND_LIGHT_GREEN);
	}

	[RelayCommand]
	public void Color7Button_Click() {
        AnyColorButtonClick(colorService.TASK_BACKGROUND_LIGHT_GRAY);

    }

	[RelayCommand]
	public void Color8Button_Click() {
        AnyColorButtonClick(colorService.TASK_BACKGROUND_DARK_GRAY);

    }

	[RelayCommand]
	public void Color9Button_Click() {
        AnyColorButtonClick(colorService.TASK_BACKGROUND_DARK_GREEN);

    }

	private void AnyColorButtonClick(Color color) {
        if (temporaryTask != null) {
			temporaryTask.DisplayColor = color;
			DidChange = true;
			OnPropertyChanged(nameof(IsSaveButtonEnabled));
		}
    }
}