namespace Timespan.GUI.Converters;

using Avalonia.Data.Converters;
using System.Globalization;

public class ButtonCornerConverter : IMultiValueConverter {
	public object? Convert(IList<object?> lst, Type targetType, object? parameter, System.Globalization.CultureInfo culture) {
		Rect bounds = (lst[0] as Rect?) ?? new Rect(0, 0, 10, 10);
		return new CornerRadius(Math.Min(bounds.Width*0.5, bounds.Height*0.23));
	}
}

public class CircleButtonCornerConverter : IMultiValueConverter {
	public object? Convert(IList<object?> lst, Type targetType, object? parameter, System.Globalization.CultureInfo culture) {
		Rect bounds = (lst[0] as Rect?) ?? new Rect(0, 0, 10, 10);
		return new CornerRadius(Math.Min(bounds.Width * 0.5, bounds.Height * 0.5));
	}
}

public class ThreeRoundedButtonCornerConverter : IMultiValueConverter {
	public object? Convert(IList<object?> lst, Type targetType, object? parameter, System.Globalization.CultureInfo culture) {
		Rect bounds = (lst[0] as Rect?) ?? new Rect(0, 0, 10, 10);
		double radius = Math.Min(bounds.Width * 0.5, bounds.Height * 0.25);
		return new CornerRadius(radius, radius, radius, 0);
	}
}