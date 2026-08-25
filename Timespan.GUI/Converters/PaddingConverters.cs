namespace Timespan.GUI.Converters;

using Avalonia;
using Avalonia.Data.Converters;
using System.Globalization;

public static class PaddingConverters {
	/// <summary>
	/// Gets a Converter that takes a number as input and converts it into a text representation
	/// </summary>

	public static FuncValueConverter<Rect, object?, Thickness> PaddingFromBoundsAndParameterConverter { get; } =
		new FuncValueConverter<Rect, object?, Thickness>(
			(rect, m) => {
				double d = 1;
				try {
					d = double.Parse((m as string) ?? "1.0", CultureInfo.InvariantCulture);
				} catch (FormatException) {
                    Console.WriteLine("format!!!");
                }
				var val = Math.Round(rect.Height * -0.30 * d, 3);
				return new Thickness(5, val, 0, val);
			}
		);
}