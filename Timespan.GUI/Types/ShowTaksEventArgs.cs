using Timespan.Types.Models;

namespace Timespan.GUI.Types; 

public class ShowTaksEventArgs(Task? task = null): EventArgs {
	public Task? Task { get; set; } = task;
}
