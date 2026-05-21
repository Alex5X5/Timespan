namespace Timespan.GUI.ViewModels;

using CommunityToolkit.Mvvm.Input;

using Timespan.GUI.Services;
using Timespan.GUI.Types;
using Timespan.GUI.ViewModels.Settings;
using Timespan.Util.Services;

public partial class SettingsViewModel : ViewModelBase, IMainViewChild {

	internal RedirectionAnchor<ISettingsViewChild> CurrentPageAnchor;
	internal ISettingsViewChild? CurrentPage => CurrentPageAnchor.CurrentModel;

	private readonly RedirectionService redirectionService;
	private readonly SettingsService settingsService;

	public SettingsViewModel(RedirectionService redirectionService, ViewModelFactory<ISettingsViewChild> factory, SettingsService settingsService) : base() {
		this.redirectionService = redirectionService;
		this.settingsService = settingsService;
		CurrentPageAnchor = new(factory);
		redirectionService.Register<SettingsViewModel, ISettingsViewChild>(CurrentPageAnchor);
		CurrentPageAnchor.ModelChanged += () => {
			OnPropertyChanged(nameof(CurrentPage));
		};
	}

	[RelayCommand]
	internal void OnSave() {
		settingsService.SaveSettings();
		redirectionService.GetAnchor<MainViewModel, IMainViewChild>()?.GoBack();
	}

	[RelayCommand]
	internal void OnCancel() {
		redirectionService.GetAnchor<MainViewModel, IMainViewChild>()?.GoBack();
	}

	internal void OnLoad() {
		CurrentPageAnchor.ChangeModel<GeneralSettingsViewModel>();
	}
}
