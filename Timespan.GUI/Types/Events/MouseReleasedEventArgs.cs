namespace Timespan.GUI.Types.Events;

public class MouseReleasedEventArgs(bool left, bool right) : EventArgs {
	
	public readonly bool Left = left;

	public readonly bool Right = right;
}
