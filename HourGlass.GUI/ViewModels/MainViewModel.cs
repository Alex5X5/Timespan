namespace Hourglass.GUI.ViewModels;

using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Timespan.Database.Services.Interfaces;
using Hourglass.GUI.ViewModels.Components.GraphPanels;
using Hourglass.GUI.ViewModels.Pages;
using Hourglass.GUI.ViewModels.Pages.SettingsPages;

using ReactiveUI;

using System.ComponentModel;
using System.Windows.Input;

public partial class MainViewModel : ViewModelBase,  INotifyPropertyChanged {

	public partial class TabButtonAction(string _text, ICommand _command, bool _selected = false) : ObservableObject {

		[ObservableProperty]
		public string text = _text;

		[ObservableProperty]
		public ICommand command = _command;
		
		[ObservableProperty]
        public bool selected = _selected;
	}

    private readonly ViewModelFactory<PageViewModelBase>? pageFactory;
	private IHourglassDbService dbService;
	private Services.CacheService cacheService;
	
	private MainViewChildPageViewModel? lastPage;
	private MainViewChildPageViewModel? _CurrentPage;
	public MainViewChildPageViewModel? CurrentPage {
		get { return _CurrentPage; }
		private set {
			Console.WriteLine($"settin current page to {value?.GetType()?.Name}");
			this.RaiseAndSetIfChanged(ref _CurrentPage, value);
			this.RaisePropertyChanged(nameof(Title));
		}
	}

	public string Title { get => (_CurrentPage as PageViewModelBase)?.Title ?? ""; }

    private GridLength navigationBarHeight = new GridLength(1, GridUnitType.Star);
	public GridLength NavigationBarHeight {
		get => navigationBarHeight;
	}
	public bool ShowNavigationBar {
		set {
			this.RaiseAndSetIfChanged(ref navigationBarHeight, value ? new GridLength(2, GridUnitType.Star) : new GridLength(0, GridUnitType.Star));
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
        
		if(cacheService!=null)
			cacheService.OnSelectedDayChanged+= date=>this.RaisePropertyChanged(nameof(Title));
    }

    public void ChangePage<PageT>(Action<PageT?>? afterCreation = null) where PageT : MainViewChildPageViewModel {
		if (pageFactory == null)
			return;
		lastPage = CurrentPage;
		CurrentPage = pageFactory.GetPageViewModel<PageT>(afterCreation) as MainViewChildPageViewModel;
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
		ChangePage<TimerPageViewModel>();
	}

	[RelayCommand]
    public void GoToGraphs() {
		ChangePage<GraphPageViewModel>(
			IsFirstGraphPageChange ? page => {
				page?.ChangeGraphPanel<DayGraphPanelViewModel>();
				IsFirstGraphPageChange = false;
			} : null
		);
	}

	[RelayCommand]
    public void GoToExport() {
		ChangePage<ExportPageViewModel>();
	}

	public void GoToTaskdetails(Database.Models.Task task) {
		ChangePage<TaskDetailsPageViewModel>();
	}

    internal void OnLoad() {
        CurrentPage = pageFactory?.GetPageViewModel<TimerPageViewModel>() as MainViewChildPageViewModel;
	}
}