namespace Timespan.GUI.ViewModels;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.ComponentModel;
using System.Threading.Tasks;

using Timespan.Database.Services;
using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;
using Timespan.GUI.ViewModels.Settings;
using Timespan.Util.Services;
public partial class MainViewModel : ViewModelBase, INotifyPropertyChanged {

	private readonly RedirectionService redirectionService;
	private readonly SettingsService settingsService;
	private readonly GuiStateService stateService;
	private readonly ITimespanDbService dbService;

	private readonly DispatcherTimer _timer;

	internal RedirectionAnchor<IMainViewChild> CurrentPageAnchor;
	internal IMainViewChild? CurrentPage => CurrentPageAnchor.CurrentModel;
	
	[ObservableProperty]
	private bool normalNavigationBarActive = true;
	[ObservableProperty]
	private bool settingsNavigationBarActive = true;

	[ObservableProperty]
	private bool showTimer = false;
	[ObservableProperty]
	private bool showBackButton = false;
	[ObservableProperty]
	private bool showSettingsButton = true;

	[ObservableProperty]
	private string timerString = "0:00:00";

	internal bool TimerButtonSelected =>
		CurrentPageAnchor.IsActive<TimerViewModel>();
	internal bool GraphsButtonSelected =>
		CurrentPageAnchor.IsActive<GraphsViewModel>();
	internal bool ExportButtonSelected =>
		CurrentPageAnchor.IsActive<ExportViewModel>();

	[ObservableProperty]
	private bool showSettingsNavigationBar = false;
	[ObservableProperty]
	private bool showNormalNavigationBar = true;

	[ObservableProperty]
	private bool showMessageOverlay = false;
	[ObservableProperty]
	private string messageOverlayString = "";

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

	public MainViewModel() : this(new RedirectionService(), new SettingsService(), new TimespanDbService(), new GuiStateService(new CacheService())) {
		
	}

	public MainViewModel(RedirectionService redirectionService, SettingsService settingsService, ITimespanDbService dbService, GuiStateService stateService) {
		this.redirectionService = redirectionService;
		this.settingsService = settingsService;
		this.dbService = dbService;
		this.stateService = stateService;
		CurrentPageAnchor = new RedirectionAnchor<IMainViewChild>();
		redirectionService.Register<MainViewModel, IMainViewChild>(CurrentPageAnchor);
		_timer = new DispatcherTimer {
			Interval = TimeSpan.FromSeconds(1)
		};
		_timer.Tick += UpdateTimer;
		CurrentPageAnchor.ModelChanged += OnPageChanged;
		CurrentPageAnchor.ModelChanged += UpdateNormalNavigationBar;
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
		if (CurrentPage.GetType() == typeof(SettingsViewModel))
			settingsService.CancelEdit();
		CurrentPageAnchor.GoBack();
	}

	private void ChangePage<PageT>(Action<PageT?>? afterCreation = null) where PageT : IMainViewChild {
		CurrentPageAnchor.ChangeModel<PageT>();
	}

	private void OnPageChanged(Type? from, Type to) {
		if (to == typeof(SettingsViewModel)) {
			ShowNormalNavigationBar = false;
			ShowSettingsNavigationBar = true;
			ShowBackButton = true;
			ShowSettingsButton = false;
			redirectionService.GetAnchor<MainViewModel, IMainViewChild>()?.ModelChanged -= UpdateNormalNavigationBar;
			redirectionService.GetAnchor<SettingsViewModel, ISettingsViewChild>()?.ModelChanged += UpdateSettingsNavigationBar;
			GlobalEventService.Raise<OpenSettingsEventArgs>();
		} else {
			ShowNormalNavigationBar = true;
			ShowSettingsNavigationBar = false;
			ShowBackButton = false;
			ShowSettingsButton = true;
			if (from == typeof(SettingsViewModel) && to != typeof(SettingsViewModel)) {
				redirectionService.GetAnchor<MainViewModel, IMainViewChild>()?.ModelChanged += UpdateNormalNavigationBar;
				redirectionService.GetAnchor<SettingsViewModel, ISettingsViewChild>()?.ModelChanged -= UpdateSettingsNavigationBar;
				GlobalEventService.Raise<CloseSettingsEventArgs>();
			}
		}
		OnPropertyChanged(nameof(CurrentPage));
	}

	private void UpdateSettingsNavigationBar(Type? from, Type to) {
		OnPropertyChanged(nameof(GeneralSettingsButtonSelected));
		OnPropertyChanged(nameof(UserDataSettingsButtonSelected));
		OnPropertyChanged(nameof(AboutSettingsButtonSelected));
		OnPropertyChanged(nameof(GraphicsSettingsButtonSelected));
		OnPropertyChanged(nameof(ExportSettingsButtonSelected));
	}

	private void UpdateNormalNavigationBar(Type? from, Type to) {
		OnPropertyChanged(nameof(TimerButtonSelected));
		OnPropertyChanged(nameof(GraphsButtonSelected));
		OnPropertyChanged(nameof(ExportButtonSelected));
	}

	internal void OnLoad() {
		CurrentPageAnchor?.ChangeModel<TimerViewModel>();
		GlobalEventService.Subscribe<TasksChangedEventArgs>(TasksChanged);
		GlobalEventService.Subscribe<ShowTaksEventArgs>(ShowTask);
		GlobalEventService.Subscribe<ShowMessageEventArgs>(ShowMessage);
		_timer.Start();
	}

	internal void OnUnload() {
		GlobalEventService.UnSubscribe<TasksChangedEventArgs>(TasksChanged);
		GlobalEventService.UnSubscribe<ShowTaksEventArgs>(ShowTask);
		GlobalEventService.UnSubscribe<ShowMessageEventArgs>(ShowMessage);
		_timer.Stop();
	}

	private void TasksChanged(TasksChangedEventArgs args) {
		UpdateTimer(this, args);
	}

	private void ShowTask(ShowTaksEventArgs args) {
		CurrentPageAnchor.ChangeModel<GraphsViewModel>();
	}

	private void ShowMessage(ShowMessageEventArgs args) {
		MessageOverlayString = args.Message;
		ShowMessageOverlay = true;
		Task.Run(
			async ()=>{
				await Task.Delay(3500);
				await Dispatcher.UIThread.InvokeAsync(HideMessage);
			});
	}

	private void HideMessage() {
		ShowMessageOverlay = false;
	}

	private async void UpdateTimer(object? sender, EventArgs args) {
		var task = await dbService.QueryCurrentTaskAsync();
		if (task == null) {
			ShowTimer = false;
			return;
		}
		task.FinishDateTime = DateTime.Now;
		await dbService.UpdateTaskAsync(task);
		stateService.RunningTask = await dbService.QueryCurrentTaskAsync();
		ShowTimer = true;
		TimerString = DateTimeService.ToHourMinuteSecondsStringAbsolute(stateService.RunningTask!.Duration);
	}
}