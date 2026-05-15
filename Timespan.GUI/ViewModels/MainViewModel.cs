namespace Timespan.GUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

using Timespan.GUI.Services;
using Timespan.GUI.ViewModels.Settings;

public partial class MainViewModel : ViewModelBase, INotifyPropertyChanged {
	
	internal RedirectionAnchor<IMainViewChild> CurrentPageAnchor;
	internal IMainViewChild? CurrentPage => CurrentPageAnchor.CurrentModel;
	
	[ObservableProperty]
	private bool normalNavigationBarActive = true;
	[ObservableProperty]
	private bool settingsNavigationBarActive = true;

	[ObservableProperty]
	internal bool showBackButton = true;

	internal bool TimerButtonSelected =>
		CurrentPageAnchor.IsActive<TimerViewModel>();
	internal bool GraphsButtonSelected =>
		CurrentPageAnchor.IsActive<GraphsViewModel>();
	internal bool ExportButtonSelected =>
		CurrentPageAnchor.IsActive<ExportViewModel>();

	[ObservableProperty]
	internal bool showSettingsNavigationBar = false;
	[ObservableProperty]
	internal bool showNormalNavigationBar = true;

	internal bool GeneralSettingsButtonSelected =>
		redirectionService.GetAnchor<SettingsViewModel, ISettingsViewChild>()?.IsActive<GeneralSettingsViewModel>() ?? false;
	internal bool UserDataSettingsButtonSelected =>
		redirectionService.GetAnchor<SettingsViewModel, ISettingsViewChild>()?.IsActive<UserDataSettingsViewModel>() ?? false;
	internal bool AboutSettingsButtonSelected =>
		redirectionService.GetAnchor<SettingsViewModel, ISettingsViewChild>()?.IsActive<AboutSettingsViewModel>() ?? false;
	internal bool GraphicsSettingsButtonSelected =>
		redirectionService.GetAnchor<SettingsViewModel, ISettingsViewChild>()?.IsActive<GraphicsSettingsViewModel>() ?? false;
	internal bool ExportSettingsButtonSelected =>
		redirectionService.GetAnchor<SettingsViewModel, ISettingsViewChild>()?.IsActive<ExportSettingsViewModel>() ?? false;
	
	private readonly RedirectionService redirectionService;

	public MainViewModel(RedirectionService redirectionService, ViewModelFactory<IMainViewChild> factory) {
		this.redirectionService = redirectionService;
		CurrentPageAnchor = new(factory);
		redirectionService.Register<MainViewModel, IMainViewChild>(CurrentPageAnchor);

		CurrentPageAnchor.ModelChanged += () => {
			if (CurrentPageAnchor?.CurrentModel?.GetType() == typeof(SettingsViewModel)) {
				ShowNormalNavigationBar = false;
				ShowSettingsNavigationBar = true;
				ShowBackButton = true;
				redirectionService.GetAnchor<SettingsViewModel, ISettingsViewChild>()?.ModelChanged += UpdateSettingsNavigationBar;
			} else {
				ShowNormalNavigationBar = true;
				ShowSettingsNavigationBar = false;
				ShowBackButton = false;
				redirectionService.GetAnchor<SettingsViewModel, ISettingsViewChild>()?.ModelChanged -= UpdateSettingsNavigationBar;
			}
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

	[RelayCommand]
	internal void GoToGeneralSettings() {
		redirectionService.GetAnchor<SettingsViewModel, ISettingsViewChild>()?.ChangeModel<GeneralSettingsViewModel>();
	}

	[RelayCommand]
	internal void GoToUserDataSettings() {
		redirectionService.GetAnchor<SettingsViewModel, ISettingsViewChild>()?.ChangeModel<UserDataSettingsViewModel>();
	}

	[RelayCommand]
	internal void GoToAboutSettings() {
		redirectionService.GetAnchor<SettingsViewModel, ISettingsViewChild>()?.ChangeModel<AboutSettingsViewModel>();
	}

	[RelayCommand]
	internal void GoToGraphicsSettings() {
		redirectionService.GetAnchor<SettingsViewModel, ISettingsViewChild>()?.ChangeModel<GraphicsSettingsViewModel>();
	}

	[RelayCommand]
	internal void GoToExportSettings() {
		redirectionService.GetAnchor<SettingsViewModel, ISettingsViewChild>()?.ChangeModel<ExportSettingsViewModel>();
	}

	[RelayCommand]
	internal void GoBack() {
		CurrentPageAnchor.GoBack();
	}

	private void ChangePage<PageT>(Action<PageT?>? afterCreation = null) where PageT : IMainViewChild {
		CurrentPageAnchor.ChangeModel<PageT>();
	}

	private void UpdateSettingsNavigationBar() {
		OnPropertyChanged(nameof(GeneralSettingsButtonSelected));
		OnPropertyChanged(nameof(UserDataSettingsButtonSelected));
		OnPropertyChanged(nameof(AboutSettingsButtonSelected));
		OnPropertyChanged(nameof(GraphicsSettingsButtonSelected));
		OnPropertyChanged(nameof(ExportSettingsButtonSelected));
	}

	private void UpdatNormalNavigationBar() {
	}

	internal void OnLoad() {
		CurrentPageAnchor.ChangeModel<TimerViewModel>();
	}
}
