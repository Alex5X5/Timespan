namespace Timespan.GUI.ViewModels;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;
using Timespan.GUI.ViewModels.Settings;
using Timespan.Util.Services;

public partial class SettingsViewModel : ViewModelBase, IMainViewChild {

	internal ScopedRedirectionAnchor<ISettingsViewChild> CurrentPageAnchor;
	internal ISettingsViewChild? CurrentPage => CurrentPageAnchor.CurrentModel;

	private readonly IServiceScopeFactory scopeFactory;
	private readonly RedirectionService redirectionService;
	private readonly SettingsService settingsService;

	public SettingsViewModel(IServiceScopeFactory scopeFactory, RedirectionService redirectionService, ScopedViewModelFactory<ISettingsViewChild> factory, SettingsService settingsService) : base() {
		this.scopeFactory = scopeFactory;
		this.redirectionService = redirectionService;
		this.settingsService = settingsService;
		CurrentPageAnchor = new ScopedRedirectionAnchor<ISettingsViewChild>(scopeFactory);
		redirectionService.Register<SettingsViewModel, ISettingsViewChild>(CurrentPageAnchor);
	}

	[RelayCommand]
	internal void OnSave() {
		settingsService.SaveSettings();
		if (settingsService.RequiresRestart) {
			Task.Run(
				async ()=> {
					MessageService.ShowMessage("The Application will exit now, to save the changes.");
					await Task.Delay(3000);
					Environment.Exit(0);
				});
		} else {
			redirectionService.GetAnchor<MainViewModel, IMainViewChild>()?.GoBack();
		}
	}

	[RelayCommand]
	internal void OnCancel() {
		settingsService.CancelEdit();
		redirectionService.GetAnchor<MainViewModel, IMainViewChild>()?.GoBack();
	}

	internal void OnLoad() {
		CurrentPageAnchor.ModelChanged += CurrentPageChanged;
		CurrentPageAnchor.CreateScope();
		CurrentPageAnchor.ChangeModel<GeneralSettingsViewModel>();
	}

	internal void OnUnLoad() {
		CurrentPageAnchor.CloseScope();
		CurrentPageAnchor.ModelChanged -= CurrentPageChanged;
	}

	private void CurrentPageChanged(Type? from, Type to) {
		OnPropertyChanged(nameof(CurrentPage));
	}
}
