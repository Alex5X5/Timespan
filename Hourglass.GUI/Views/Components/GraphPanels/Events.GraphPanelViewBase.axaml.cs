namespace Hourglass.GUI.Views.Components.GraphPanels;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

using Timespan.Database.Models;
using Hourglass.GUI.ViewModels.Components.GraphPanels;

public abstract partial class GraphPanelViewBase : ViewBase {

	public async void OnClickBase(object? sender, TappedEventArgs e) {
		Console.WriteLine("base graph panel click");
		Point mousePos = e.GetPosition(this);
		if (IsOutsideGraphArea(mousePos))
			return;
		if (DataContext is GraphPanelViewModelBase model) {
			int i = 0;
			Task? clickedTask = null;
			foreach (var task in await model.GetTasksAsync()) {
				Rect rect = GetTaskRectanlge(task, GRAPH_CLICK_ADDITIONAL_WIDTH, GRAPH_CLICK_ADDITIONAL_HEIGHT, i);
				i++;
				if (rect.Contains(mousePos)) {
					clickedTask = task;
					break;
				} else {
					continue;
				}
			}
			if (clickedTask != null)
				model.OnTaskClicked(clickedTask);
		}
		OnClick(sender, e);
	}

	protected void OnDoubleClickBase(object? sender, TappedEventArgs args) {
		Console.WriteLine("base graph panel double click");
		Point mousePos = args.GetPosition(this);
		if (IsOutsideGraphArea(mousePos))
			return;
		double offset = mousePos.X - PADDING_X;
		double clickSeconds = TIME_INTERVALL_START_SECONDS + TIME_INTERVALL_DURATION * offset / (Bounds.Width - 2 * PADDING_X);
		DateTime clickDate = new DateTime((long)clickSeconds * TimeSpan.TicksPerSecond);
		(DataContext as GraphPanelViewModelBase)?.OnDoubleClick(clickDate);
	}

	private void OnMouseMovedBase(object sender, PointerEventArgs args) {
		MousePos = args.GetCurrentPoint(this).Position;
		if (RightMouseDown) {
			MarkerDragRectangle = new Rect(
				Math.Min(MousePos.X, DragOrigin.X),
				Math.Min(MousePos.Y, DragOrigin.Y),
				Math.Abs(MousePos.X - DragOrigin.X),
				Math.Abs(MousePos.Y - DragOrigin.Y)
			);
			(DataContext as GraphPanelViewModelBase)?.OnMouseDragging(MarkerDragRectangle, GRAPH_AREA_WIDTH, PADDING_X);
		} else {
			(DataContext as GraphPanelViewModelBase)?.OnMouseMoved();
		}
		OnMouseMoved(sender, args);
		InvalidateVisual();
	}

	private void OnMousePressedBase(object sender, PointerPressedEventArgs args) {
		PointerPoint mousePoint = args.GetCurrentPoint(sender as Control);
		MousePos = mousePoint.Position;
		DragOrigin = mousePoint.Position;
		Console.WriteLine($"mouse pressed!");
		if (mousePoint.Properties.IsRightButtonPressed) {
			if (!RightMouseDown)
				DragOrigin = MousePos;
			RightMouseDown = true;
			MarkerDragRectangle = new Rect(
				Math.Min(MousePos.X, DragOrigin.X),
				Math.Min(MousePos.Y, DragOrigin.Y),
				Math.Abs(MousePos.X - DragOrigin.X),
				Math.Abs(MousePos.Y - DragOrigin.Y)
			);
			InvalidateVisual();
		}
		if (mousePoint.Properties.IsLeftButtonPressed)
			LeftMouseDown = true;
		Model.OnMousePressed(mousePoint.Properties.IsLeftButtonPressed, mousePoint.Properties.IsRightButtonPressed);
		InvalidateVisual();
	}

	private void OnMouseReleasedBase(object sender, PointerReleasedEventArgs args) {
		Console.WriteLine($"mouse released!");
		if (!args.GetCurrentPoint(sender as Control).Properties.IsRightButtonPressed) {
			if (!IsOutsideGraphArea(MousePos))
				if (RightMouseDown) {
					ShowReasonContextMenu();
				}
			RightMouseDown = false;
		}
		if (!args.GetCurrentPoint(sender as Control).Properties.IsLeftButtonPressed)
			LeftMouseDown = false;
		InvalidateVisual();
	}
	
	private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		(DataContext as GraphPanelViewModelBase)?.OnLoad();
		//TasksGrid = grid;
    //    if (DataContext is GraphPanelViewModelBase model)
    //        if(TasksGrid != null)
				//TasksGrid.RowDefinitions = RowDefinitions.Parse(model.Rows);

        //if (DataContext is GraphPanelViewModelBase model)
        //model.NotifyTransitionStep += (s) => {
        //	KeyFrame CreateKeyFrame(double cue, double opacity, int zIndex, bool isVisible = true) =>
        //		new() {
        //			Setters = {
        //				new Setter { Property = Visual.OpacityProperty, Value = opacity },

        //				new Setter { Property = Visual.RenderTransformProperty, Value = TransformOperations.Parse("translateX(100px)")}
        //			},
        //			Cue = new Cue(cue)
        //		};

        //	var animation = new Animation {
        //		Duration = new TimeSpan(1 * TimeSpan.TicksPerSecond),
        //		FillMode = FillMode.Forward,
        //		Children = {
        //			CreateKeyFrame(0d, 0.0d, 1),
        //			CreateKeyFrame(0.5d, 0.5, 1),
        //			CreateKeyFrame(1d, 1.0d, 2)
        //		}
        //	};
        //	//animation.RunAsync(this);
        //};
        InvalidateVisual();
	}

	public virtual void OnClick(object? sender, TappedEventArgs e) {
		//Console.WriteLine("closed context menu!");
	}

	private void PreviusIntervallClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		(DataContext as GraphPanelViewModelBase)?.PreviusIntervallClickBase();
		InvalidateVisual();
	}

	private void FollowingIntervallClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		(DataContext as GraphPanelViewModelBase)?.FollowingIntervallClickBase();
		InvalidateVisual();
	}

	public virtual void OnDoubleClick(object? sender, TappedEventArgs e) { }

	public virtual void OnMouseMoved(object? sender, PointerEventArgs e) { }

	public virtual void OnMousePressed(object sender, PointerPressedEventArgs args) { }

	public virtual void OnMouseReleased(object sender, PointerReleasedEventArgs args) { }
}