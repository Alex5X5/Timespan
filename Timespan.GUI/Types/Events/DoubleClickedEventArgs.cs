namespace Timespan.GUI.Types.Events;

public class DoubleClickedEventArgs(int row, int col) : EventArgs {

	public int Row { get; private set; } = row;	
	public int Col { get; private set; } = col;
}
