namespace Timespan.GUI.ViewModels;

using CommunityToolkit.Mvvm.Input;

using Timespan.GUI.Services;
using Timespan.GUI.ViewModels.Settings;

public partial class SettingsViewModel : ViewModelBase, IMainViewChild {

	internal RedirectionAnchor<ISettingsViewChild> CurrentPageAnchor;
	internal ISettingsViewChild? CurrentPage => CurrentPageAnchor.CurrentModel;

	private readonly RedirectionService redirectionService;

	public SettingsViewModel(RedirectionService redirectionService, ViewModelFactory<ISettingsViewChild> factory) : base() {
		this.redirectionService = redirectionService;
		CurrentPageAnchor = new(factory);
		redirectionService.Register<SettingsViewModel, ISettingsViewChild>(CurrentPageAnchor);
		CurrentPageAnchor.ModelChanged += () => {
			OnPropertyChanged(nameof(CurrentPage));
		};
	}

	[RelayCommand]
	internal void GoToGeneral() {
		ChangePage<GeneralSettingsViewModel>();
	}

	private void ChangePage<PageT>(Action<PageT?>? afterCreation = null) where PageT : ISettingsViewChild {
		CurrentPageAnchor.ChangeModel<PageT>();
	}

	internal void OnLoad() {
		CurrentPageAnchor.ChangeModel<GeneralSettingsViewModel>();
	}
}
