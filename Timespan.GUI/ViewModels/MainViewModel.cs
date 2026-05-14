namespace Timespan.GUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

using Timespan.GUI.Services;

internal partial class MainViewModel : ViewModelBase, INotifyPropertyChanged {

	
	internal RedirectionAnchor<IMainViewChild> CurrentPageAnchor;
	internal IMainViewChild? CurrentPage => CurrentPageAnchor.CurrentModel;
	
	[ObservableProperty]
	private bool normalNavigationBarActive = true;
	[ObservableProperty]
	private bool settingsNavigationBarActive = true;

	internal bool TimerButtonSelected =>
		CurrentPageAnchor.IsActive<TimerViewModel>();
	internal bool GraphsButtonSelected =>
		CurrentPageAnchor.IsActive<GraphsViewModel>();
	internal bool ExportButtonSelected =>
		CurrentPageAnchor.IsActive<ExportViewModel>();

	[ObservableProperty]
	private bool generalSettingsButtonSelected = true;
	[ObservableProperty]
	private bool userDataSettingsButtonSelected = false;
	[ObservableProperty]
	private bool aboutSettingsButtonSelected = false;
	[ObservableProperty]
	private bool visualSettingsButtonSelected = false;
	[ObservableProperty]
	private bool exportSettingsButtonSelected = false;

	private readonly RedirectionService redirectionService;

	internal MainViewModel(RedirectionService redirectionService) {
		this.redirectionService = redirectionService;
		CurrentPageAnchor = new([new TimerViewModel(), new GraphsViewModel(), new ExportViewModel(), new SettingsViewModel(redirectionService)]);
		redirectionService.RegisterRedirectionAnchor<MainViewModel, IMainViewChild>(CurrentPageAnchor);
		CurrentPageAnchor.ModelChanged += () => {
			OnPropertyChanged(nameof(TimerButtonSelected));
			OnPropertyChanged(nameof(GraphsButtonSelected));
			OnPropertyChanged(nameof(ExportButtonSelected));
			OnPropertyChanged(nameof(CurrentPage));
		};
	}

	[RelayCommand]
	internal void GoToTimer() {
		ChangePage<TimerViewModel>();
	}

	[RelayCommand]
	internal void GoToGraphs() {
		ChangePage<GraphsViewModel>();
	}

	[RelayCommand]
	internal void GoToExport() {
		ChangePage<ExportViewModel>();
	}

	[RelayCommand]
	internal void GoToSettings() {
		ChangePage<SettingsViewModel>();
	}

	private void ChangePage<PageT>(Action<PageT?>? afterCreation = null) where PageT : IMainViewChild {
		CurrentPageAnchor.ChangeModel<PageT>();
		//lastPage = CurrentPage;
		//CurrentPage = Pages.First(x => x.GetType() == typeof(PageT));
		//CurrentPage = pageFactory.GetPageViewModel<PageT>(afterCreation) as IMainViewChild;
		//Console.WriteLine($"chaged type of page to:{_CurrentPage?.GetType()?.Name ?? "NullType"}");
		//Console.WriteLine($"new page is {_CurrentPage?.GetType()?.IsVisible ?? false} visible");
	}
}
