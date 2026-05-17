namespace Hourglass.GUI.ViewModels.Pages.SettingsPages;

using Timespan.Database.Services.Interfaces;
using Hourglass.Util;
using ReactiveUI;

public abstract class SubSettingsPageViewModelBase : ViewModelBase {

    protected DateTimeService dateTimeService { set; get; }
	protected SettingsService settingsService;


	protected MainViewModel pageController;

	public abstract string Title { get; }

	private bool hasUnsavedChanges = false;
	public bool HasUnsavedChanges {
		protected set => this.RaiseAndSetIfChanged(ref hasUnsavedChanges, value);
		get => hasUnsavedChanges;
	}

    public SubSettingsPageViewModelBase() : this(null, null, null) {
		
	}
	
	public SubSettingsPageViewModelBase(DateTimeService dateTimeService, MainViewModel pageController, SettingsService settingsService) : base() {
        this.dateTimeService = dateTimeService;
		this.pageController = pageController;
		this.settingsService = settingsService;
        if (settingsService != null)
            settingsService.OnPreSettingsSave += SaveSettings;
		HasUnsavedChanges = false;
    }

    public abstract void SaveSettings();
}
	