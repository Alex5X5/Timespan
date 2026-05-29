namespace Timespan.GUI.ViewModels;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

using Timespan.GUI.Services;
using Timespan.GUI.ViewModels.Settings;
using Timespan.Util.Services;

public partial class SettingsViewModel : ViewModelBase, IMainViewChild {

	internal RedirectionAnchor<ISettingsViewChild> CurrentPageAnchor;
	internal ISettingsViewChild? CurrentPage => CurrentPageAnchor.CurrentModel;

	private readonly IServiceScopeFactory scopeFactory;
	private readonly RedirectionService redirectionService;
	private readonly SettingsService settingsService;

	public SettingsViewModel(IServiceScopeFactory scopeFactory, RedirectionService redirectionService, ViewModelFactory<ISettingsViewChild> factory, SettingsService settingsService) : base() {
		this.scopeFactory = scopeFactory;
		this.redirectionService = redirectionService;
		this.settingsService = settingsService;
		CurrentPageAnchor = new(factory);
		redirectionService.Register<SettingsViewModel, ISettingsViewChild>(CurrentPageAnchor);
		CurrentPageAnchor.ModelChanged += (from, to) => {
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
