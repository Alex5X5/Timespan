using Avalonia.Interactivity;

namespace Timespan.GUI.Types.Events;

public sealed class ColorSelectedEventArgs : RoutedEventArgs {
	public Color Color {
		get;
	}

	public ColorSelectedEventArgs(
		RoutedEvent routedEvent,
		object source,
		Color color)
		: base(routedEvent, source) {
		Color = color;
	}
}
