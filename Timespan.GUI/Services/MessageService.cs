using Avalonia.Threading;

using Timespan.GUI.Types.Events;

namespace Timespan.GUI.Services;

internal static class MessageService {

	internal static void ShowMessage(string message) {
		var args = new ShowMessageEventArgs(message);
		Dispatcher.UIThread.Invoke(
			()=>GlobalEventService.Raise(args));
	}
}
