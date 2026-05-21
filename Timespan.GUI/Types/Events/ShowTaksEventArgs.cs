namespace Timespan.GUI.Types.Events; 

using Timespan.Types.Models;

public class ShowTaksEventArgs(Task? task = null): EventArgs {
	public Task? Task { get; set; } = task;
}
