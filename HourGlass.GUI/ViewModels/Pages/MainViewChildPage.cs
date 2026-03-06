using ReactiveUI;
using System.Collections.ObjectModel;
using static Hourglass.GUI.ViewModels.MainViewModel;

namespace Hourglass.GUI.ViewModels.Pages;

public abstract class MainViewChildPageViewModel : PageViewModelBase {
	
	public ObservableCollection<TabButtonAction> ButtonActions {
		protected set;
		get;
	}

	public MainViewChildPageViewModel() : base() {
		ButtonActions = new ObservableCollection<TabButtonAction>() {
			new TabButtonAction(
				TranslatorService.Singleton["Views.MainView.Buttons.Timer"] ?? "Timer",
				ReactiveCommand.Create(
					()=>{
						SetTabButtonSeleted(0);
					}
				),
				true
			),
			new TabButtonAction(
				TranslatorService.Singleton["Views.MainView.Buttons.Graphs"] ?? "Graphs",
				ReactiveCommand.Create(
					()=>SetTabButtonSeleted(1)
				)
			),
			new TabButtonAction(TranslatorService.Singleton["Views.MainView.Buttons.Export"] ?? "Export", ReactiveCommand.Create(()=>SetTabButtonSeleted(2))),
		};
	}

	protected void SetTabButtonSeleted(int index) {
		foreach (var action in ButtonActions)
			action.Selected = action == ButtonActions[index];
	}

}
