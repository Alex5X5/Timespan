namespace Hourglass.GUI.ViewModels;

using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

using Hourglass.Database.Services.Interfaces;
using Hourglass.GUI.ViewModels.Components.GraphPanels;
using Hourglass.GUI.ViewModels.Pages;
using Hourglass.GUI.ViewModels.Pages.SettingsPages;

using ReactiveUI;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

public partial class MainViewModel : ViewModelBase,  INotifyPropertyChanged {
	
    public record class ButtonAction(string Text, ICommand Command);

    private readonly ViewModelFactory<PageViewModelBase>? pageFactory;
	private IHourglassDbService dbService;
	private Services.CacheService cacheService;

	private PageViewModelBase? lastPage;

	private PageViewModelBase? _CurrentPage;
	public PageViewModelBase? CurrentPage {
		get { return _CurrentPage; }
		private set {
			Console.WriteLine($"settin current page to {value?.GetType()?.Name}");
			this.RaiseAndSetIfChanged(ref _CurrentPage, value);
			this.RaisePropertyChanged(nameof(Title));
		}
	}

	public string Title { get => _CurrentPage?.Title ?? ""; }

    private GridLength navigationBarHeight = new GridLength(1, GridUnitType.Star);
	public GridLength NavigationBarHeight {
		get => navigationBarHeight;
	}
	public bool ShowNavigationBar {
		set {
			this.RaiseAndSetIfChanged(ref navigationBarHeight, value ? new GridLength(2, GridUnitType.Star) : new GridLength(0, GridUnitType.Star));
			this.RaisePropertyChanged(nameof(NavigationBarHeight));
		}
	}

    private bool showSettingsIcon = false;
    public bool ShowSettingsIcon {
        set {
            this.RaiseAndSetIfChanged(ref showSettingsIcon, value);
        }
        get => showSettingsIcon;
    }

    private bool timerButtonSelected = true;
    public bool TimerButtonSelected {
        set {
            this.RaiseAndSetIfChanged(ref timerButtonSelected, value);
        }
        get => timerButtonSelected;
    }

    private bool graphsButtonSelected = false;
    public bool GraphsButtonSelected {
        set {
            this.RaiseAndSetIfChanged(ref graphsButtonSelected, value);
        }
        get => graphsButtonSelected;
    }

    private bool exportButtonSelected = false;
    public bool ExportButtonSelected {
        set {
            this.RaiseAndSetIfChanged(ref exportButtonSelected, value);
        }
        get => exportButtonSelected;
    }

    private bool IsFirstGraphPageChange = true;

    public MainViewModel() : this(null, null, null, null) {
		
	}

	public MainViewModel(IHourglassDbService dbService, DateTimeService dateTimeService, ViewModelFactory<PageViewModelBase> pageFactory, Services.CacheService cacheService) : base() {
		this.dbService = dbService;
		this.pageFactory = pageFactory;
		this.cacheService = cacheService;

		ShowSettingsIcon = true;
		ShowNavigationBar = true;
        
		CurrentPage = pageFactory?.GetPageViewModel<TimerPageViewModel>();
		if(cacheService!=null)
			cacheService.OnSelectedDayChanged+= date=>this.RaisePropertyChanged(nameof(Title));
    }

    public void ChangePage<PageT>(Action<PageT?>? afterCreation = null) where PageT : PageViewModelBase {
		if (pageFactory == null)
			return;
		lastPage = CurrentPage;
		CurrentPage = pageFactory.GetPageViewModel<PageT>(afterCreation);
		Console.WriteLine($"chaged type of page to:{_CurrentPage?.GetType()?.Name ?? "NullType"}");
		Console.WriteLine($"new page is {_CurrentPage?.GetType()?.IsVisible ?? false} visible");
	}

	public void GoBack() {
		if (lastPage != null) {
			CurrentPage = lastPage;
			lastPage = null;
		}
	}

	[RelayCommand]
    public void GoToSettings() {
		ChangePage<SettingsPageViewModel>(
			page => {
				page?.ChangePage<GeneralSubSettingsPageViewModel>();
			}
		);
	}

	[RelayCommand]
	public void GoToTimer() {
        TimerButtonSelected = true;
        GraphsButtonSelected = false;
        ExportButtonSelected = false;
		ChangePage<TimerPageViewModel>();
	}

	[RelayCommand]
    public void GoToGraphs() {
		TimerButtonSelected = false;
		GraphsButtonSelected = true;
		ExportButtonSelected = false;
		ChangePage<GraphPageViewModel>(
			IsFirstGraphPageChange ? page=> {
				page?.ChangeGraphPanel<DayGraphPanelViewModel>();
				IsFirstGraphPageChange = false;
			} : null
		);
	}

	[RelayCommand]
    public void GoToExport() {
        TimerButtonSelected = false;
        GraphsButtonSelected = false;
        ExportButtonSelected = true;
		ChangePage<ExportPageViewModel>();
	}

	public void GoToTaskdetails(Database.Models.Task task) {
		if(pageFactory==null)
			return;
		ChangePage<TaskDetailsPageViewModel>();
	}
}