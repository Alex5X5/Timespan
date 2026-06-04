namespace Timespan.GUI.Types.Events;

public class MouseDraggingEventArgs(Rect rect, double width, double paddingX) : EventArgs {
	
	public readonly Rect DragRectangle = rect;

	public readonly double Width = width;

	public readonly double PaddingX = paddingX;
}
