namespace Hourglass.GUI.ViewModels.Pages;

using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using System.ComponentModel;

using Hourglass.GUI.ViewModels.Pages.SettingsPages;

public partial class SettingsPageViewModel : PageViewModelBase, INotifyPropertyChanged {

    private SettingsService settingsService;
    private ViewModelFactory<SubSettingsPageViewModelBase> pageFactory;
    MainViewModel controller;

    private SubSettingsPageViewModelBase? _CurrentSubSettingsPage;
    public SubSettingsPageViewModelBase? CurrentPage {
        get { return _CurrentSubSettingsPage; }
        private set {
            Console.WriteLine($"settin current sub settings page to {value?.GetType()?.Name}");
            if(_CurrentSubSettingsPage != null)
                _CurrentSubSettingsPage.PropertyChanged -= OnUnsavedChangesChanged;
            this.RaiseAndSetIfChanged(ref _CurrentSubSettingsPage, value);
            if (_CurrentSubSettingsPage != null)
                _CurrentSubSettingsPage.PropertyChanged += OnUnsavedChangesChanged;
            this.RaisePropertyChanged(nameof(Title));
        }
    }

    private void OnUnsavedChangesChanged(object? sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(CurrentPage.HasUnsavedChanges)) {
            this.RaiseAndSetIfChanged(ref isSaveButtonEnabled, CurrentPage?.HasUnsavedChanges ?? false);
        }
    }

    public override string Title => _CurrentSubSettingsPage?.Title ?? "";

    private bool isSaveButtonEnabled = false;
    public bool IsSaveButtonEnabled {
        get => isSaveButtonEnabled;
        set {
            this.RaiseAndSetIfChanged(ref isSaveButtonEnabled, value);
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public SettingsPageViewModel() : this(null, null, null) {
		
	}

	public SettingsPageViewModel(ViewModelFactory<SubSettingsPageViewModelBase> pageFactory, MainViewModel controller, SettingsService settingsService) : base() {
        this.controller = controller;
        this.pageFactory = pageFactory;
        this.settingsService = settingsService;
    }

    private void AllBindingPropertiesChanged() {
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(IsSaveButtonEnabled));
        controller.RaisePropertyChanged(nameof(controller.Title));
    }

    protected virtual void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    [RelayCommand]
    public void Cancel() {
        controller.ShowNavigationBar = true;
        controller.ShowSettingsIcon = true;
        controller.GoBack();
    }

    [RelayCommand]
    public void Save() {
        CurrentPage?.SaveSettings();
        settingsService.SaveSettings();
        Cancel();
    }

    [RelayCommand]
    private void GoToGeneral() {
        ChangePage<GeneralSubSettingsPageViewModel>();
    }

    [RelayCommand]
    private void GoToAbout() {
        ChangePage<AboutSubSettingsPageViewModel>();
    }

    [RelayCommand]
    private void GoToVisuals() {
        ChangePage<VisualsSubSettingsPageViewModel>();
    }

    [RelayCommand]
    private void GoToUserData() {
        ChangePage<UserDataSubSettingsPageViewModel>();
    }

    [RelayCommand]
    private void GoToExport() {
        ChangePage<ExportSubSettingsPageViewModel>();
    }

    public void OnLoad() {
        Console.WriteLine("loading Settings Page");
        AllBindingPropertiesChanged();
        controller.ShowNavigationBar = false;
        controller.ShowSettingsIcon = false;
    }

    public void ChangePage<PageT>(Action<PageT?>? afterCreation = null) where PageT : SubSettingsPageViewModelBase {
        if (pageFactory == null)
            return;
        CurrentPage = pageFactory.GetPageViewModel<PageT>(afterCreation);
        AllBindingPropertiesChanged();
        Console.WriteLine($"chaged type of sub settings page to:{_CurrentSubSettingsPage?.GetType()?.Name ?? ""}");
    }
}

