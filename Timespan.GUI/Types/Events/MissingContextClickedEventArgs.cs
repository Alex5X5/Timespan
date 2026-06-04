using Timespan.Types.Models;

namespace Timespan.GUI.Types.Events;

public class MissingContextClickedEventArgs(BlockedTimeIntervallType reason) : EventArgs {

	public readonly BlockedTimeIntervallType Reason = reason;
}
