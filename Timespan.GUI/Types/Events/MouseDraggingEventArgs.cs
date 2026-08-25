namespace Timespan.GUI.Types.Events;

public class MouseDraggingEventArgs(Rect rect, double width, double height, double paddingX, double paddingY) : EventArgs {
	
	public readonly Rect DragRectangle = rect;

	public readonly double Width = width;

	public readonly double Height = height;

	public readonly double PaddingX = paddingX;

	public readonly double PaddingY = paddingY;
}
