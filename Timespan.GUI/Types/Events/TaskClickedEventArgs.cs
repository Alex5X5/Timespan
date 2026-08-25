namespace Timespan.GUI.Types.Events;

public class TaskClickedEventArgs(Timespan.Types.Models.Task task) : EventArgs {
	
	public readonly Timespan.Types.Models.Task Task = task;
}
