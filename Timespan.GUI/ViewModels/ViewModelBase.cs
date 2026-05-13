namespace Timespan.GUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public class ViewModelBase : ObservableObject {
	
	internal ViewModelBase() : base() {
		Console.WriteLine($"constructing ViewModelBase for view model type '{GetType().Name}'");
    }
}
