namespace Timespan.GUI.Types.Events; 

using Timespan.Types.Models;

public class SelectedtaskChangedEventArgs(Task? task = null): ShowTaksEventArgs(task) {
}
