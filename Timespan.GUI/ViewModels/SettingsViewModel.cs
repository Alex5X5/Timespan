namespace Timespan.GUI.ViewModels;

using CommunityToolkit.Mvvm.Input;

using Timespan.GUI.Services;
using Timespan.GUI.ViewModels.Settings;

internal partial class SettingsViewModel : ViewModelBase, IMainViewChild {


	internal RedirectionAnchor<ISettingsViewChild> CurrentPageAnchor;
	internal ISettingsViewChild? CurrentPage => CurrentPageAnchor.CurrentModel;

	private readonly RedirectionService redirectionService;

	internal SettingsViewModel(RedirectionService redirectionService) : base() {
		this.redirectionService = redirectionService;
		CurrentPageAnchor = new([new GeneralViewModel()]);
		redirectionService.RegisterRedirectionAnchor<MainViewModel, ISettingsViewChild>(CurrentPageAnchor);
		CurrentPageAnchor.ModelChanged += () => {
		};
	}

	[RelayCommand]
	internal void GoToGeneral() {
		ChangePage<GeneralViewModel>();
	}

	private void ChangePage<PageT>(Action<PageT?>? afterCreation = null) where PageT : ISettingsViewChild {
		CurrentPageAnchor.ChangeModel<PageT>();
	}
}
