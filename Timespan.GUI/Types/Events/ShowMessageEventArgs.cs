namespace Timespan.GUI.Types.Events;

public class ShowMessageEventArgs(string message) : EventArgs {

	public readonly string Message = message;
}
