namespace Timespan.GUI.ViewModels;

public class MainViewModel : ViewModelBase {

	private List<IMainViewChild> Pages { get; set; }
	private IMainViewChild _CurrentPage;
	internal IMainViewChild CurrentPage {
		get => _CurrentPage;
		private set {
			Console.WriteLine($"settin current page to {value.GetType().Name}");
			this.RaiseAndSetIfChanged(ref _CurrentPage, value);
		}
	}

	public MainViewModel() {
		Pages = [new TimerViewModel(), new ExportViewModel()];
		CurrentPage = Pages[0];
	}
}
