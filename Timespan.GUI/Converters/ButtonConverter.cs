namespace Timespan.GUI.Converters;

using Avalonia.Data.Converters;
using System.Globalization;

public class ButtonCornerConverter : IMultiValueConverter {
	public object? Convert(IList<object?> lst, Type targetType, object? parameter, System.Globalization.CultureInfo culture) {
		Rect bounds = (lst[0] as Rect?) ?? new Rect(0, 0, 10, 10);
		return new CornerRadius(Math.Min(bounds.Width*0.5, bounds.Height*0.23));
	}
}

public class ThreeRoundedButtonCornerConverter : IValueConverter {

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        Rect bounds = (value as Rect?) ?? new Rect(0, 0, 10, 10);
        double radius = Math.Min(bounds.Width, bounds.Height) * 0.15;
        return new CornerRadius(radius, radius, radius, 0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}