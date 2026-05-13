namespace Timespan.GUI.ViewModels;

public class ViewModelBase : ReactiveObject {
	
	internal ViewModelBase() : base() {
		Console.WriteLine($"constructing ViewModelBase for view model type '{GetType().Name}'");
    }
}
