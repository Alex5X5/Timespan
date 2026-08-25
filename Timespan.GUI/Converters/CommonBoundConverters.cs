namespace Timespan.GUI.Converters;

using Avalonia.Data.Converters;

public class CommonBoundConverters : IMultiValueConverter {
	public object? Convert(IList<object?> lst, Type targetType, object? parameter, System.Globalization.CultureInfo culture) {
		Rect bounds = (lst[0] as Rect?) ?? new Rect(0, 0, 10, 10);
		return new CornerRadius(Math.Min(bounds.Width*0.5, bounds.Height*0.23));
	}
}