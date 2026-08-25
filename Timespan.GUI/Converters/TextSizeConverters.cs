namespace Timespan.GUI.Converters;

using Avalonia;
using Avalonia.Data.Converters;
using System.Globalization;

public static class TextSizeConverters {
	/// <summary>
	/// Gets a Converter that takes a number as input and converts it into a text representation
	/// </summary>
	public static FuncValueConverter<Rect, object?, double> TextSizeFromWidthAndHeightAndParameterConverter { get; } =
		new FuncValueConverter<Rect, object?, double>(
			(rect, m) => {
				double d = double.Parse((m as string)!, CultureInfo.InvariantCulture);
                var val = Math.Round(rect.Height * 0.55 * d, 1);
				return Math.Max(5.0, val);
			}
		);

	public static FuncValueConverter<Rect, double> TextSizeFromWidthAndHeightConverter { get; } =
		new FuncValueConverter<Rect, double>(
			rect => {
				var val = Math.Round(rect.Height * 0.55, 1);
				return Math.Max(5.0, val);
			}
		);

	public static FuncValueConverter<Rect, int, double> TextSizeFromWidthAndHeightConverterMultiLine { get; } =
		new FuncValueConverter<Rect, int, double>(
			(rect, lineCount) => {
				double val = Math.Round(rect.Height / lineCount, 1);
				return 30;
			}
		);

	public static FuncMultiValueConverter<string?, string> MyMultiConverter { get; } =
		new FuncMultiValueConverter<string?, string>(num => $"Your numbers are: '{string.Join(", ", num)}'");
}