using System.Collections.ObjectModel;
using static Hourglass.GUI.ViewModels.MainViewModel;

namespace Hourglass.GUI.ViewModels.Pages;

public abstract class PageViewModelBase : ViewModelBase {

	public abstract string Title { get; }

    public ObservableCollection<ButtonAction> ButtonActions { protected set;  get; }

    public PageViewModelBase() : base() {
			
	}

}
