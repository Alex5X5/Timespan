namespace Hourglass.GUI.Views.Components;

using Avalonia;
using Avalonia.Controls;
using Hourglass.GUI.ViewModels.Components;

public partial class TaskGraph : UserControl {

	public static StyledProperty<double> GaphWidthProperty = AvaloniaProperty.Register<TaskGraph, double>(nameof(GraphWidth));
	
	public double GraphWidth {
        get => GetValue(GaphWidthProperty);
        set => SetValue(GaphWidthProperty, value);
    }
	
	public bool IsRemoving {
		get => (DataContext as TaskGraphViewModel)?.IsRemoving ?? false;
		//set {
		//	if (DataContext is TaskGraphViewModel model)
		//		model.IsRemoving = value;
		//}
	}

	public Database.Types.Task? Task {
		set; get;
	}

	public TaskGraph() : base(){
		InitializeComponent();
	}

	private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		//if (DataContext is TaskGraphViewModel model)
			//model.PropertyChanged +=
			//	(name, args) => {
			//		if (args.PropertyName == nameof(TaskGraphViewModel.IsRemoving))
			//			if (IsRemoving && this.FindControl<Rectangle>("Rect") is { } rect) {
			//				rect.IsVisible = false;
			//				rect.
			//				InvalidateVisual();
			//			}
			//	};
	}
}