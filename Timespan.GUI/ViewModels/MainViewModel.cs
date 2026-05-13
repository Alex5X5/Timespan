namespace Timespan.GUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

using System.Linq;

public partial class MainViewModel : ViewModelBase, INotifyPropertyChanged {

	private List<IMainViewChild> Pages { get; set; }

	private IMainViewChild? lastPage;
	[ObservableProperty]
	private IMainViewChild? currentPage;

	[ObservableProperty]
	private bool timerButtonSelected = true;
	[ObservableProperty]
	private bool graphsButtonSelected = false;
	[ObservableProperty]
	private bool exportButtonSelected = false;

	public MainViewModel() {
		Pages = [new TimerViewModel(), new GraphsViewModel(), new ExportViewModel()];
		CurrentPage = Pages[0];
	}

	[RelayCommand]
	public void GoToTimer() {
		ChangePage<TimerViewModel>();
		TimerButtonSelected = true;
		GraphsButtonSelected = false;
		ExportButtonSelected = false;
	}

	[RelayCommand]
	public void GoToGraphs() {
		ChangePage<GraphsViewModel>();
		TimerButtonSelected = false;
		GraphsButtonSelected = true;
		ExportButtonSelected = false;
	}

	[RelayCommand]
	public void GoToExport() {
		ChangePage<ExportViewModel>();
		TimerButtonSelected = false;
		GraphsButtonSelected = false;
		ExportButtonSelected = true;
	}

	private void ChangePage<PageT>(Action<PageT?>? afterCreation = null) where PageT : IMainViewChild {
		lastPage = CurrentPage;
		CurrentPage = Pages.First(x => x.GetType() == typeof(PageT));
		//CurrentPage = pageFactory.GetPageViewModel<PageT>(afterCreation) as IMainViewChild;
		//Console.WriteLine($"chaged type of page to:{_CurrentPage?.GetType()?.Name ?? "NullType"}");
		//Console.WriteLine($"new page is {_CurrentPage?.GetType()?.IsVisible ?? false} visible");
	}
}
