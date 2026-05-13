namespace Timespan.GUI.Converters;

using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;

public static class ColorConverters {
	/// <summary>
	/// Gets a Converter that takes a number as input and converts it into a text representation
	/// </summary>
	public static FuncValueConverter<decimal?, string> MyConverter {
		get;
	} =
		new FuncValueConverter<decimal?, string>(num => $"is: '{num}'");

	public static FuncValueConverter<IBrush?, IBrush> ColorConverterBrighten { get; } =
		new FuncValueConverter<IBrush?, IBrush>(
			br => {
				if (br is ImmutableSolidColorBrush immutableSolidBrush) {
					return new SolidColorBrush(
						TransformColor(immutableSolidBrush.Color, 2.0f, 1.25f, 1.2f)
					);
				}
				if (br is SolidColorBrush solidBrush) {
					return new SolidColorBrush(
						TransformColor(solidBrush.Color, 2.0f, 1.25f, 1.2f)
					);
				}
				return br ?? Brushes.Transparent;
			}
		);

	public static FuncValueConverter<IBrush?, IBrush> ColorConverterDarken { get; } =
		new FuncValueConverter<IBrush?, IBrush>(
			br => {
				if (br is ImmutableSolidColorBrush immutableSolidBrush) {
					return new SolidColorBrush(
						TransformColor(immutableSolidBrush.Color, 0.3f, 0.6f, 0.7f)
					);
				}
				if (br is SolidColorBrush solidBrush) {
					return new SolidColorBrush(
						TransformColor(solidBrush.Color, 3.0f, 1.5f, 1.4f)
					);
				}
				return br ?? Brushes.Transparent;
			}
		);

	private static Color TransformColor(Color color, float transformLowest, float transfomrMid, float transformHighest) {
		byte a = color.R, b = color.G, c = color.B;
		if (a==b && b==c)
            return new Color(
                color.A,
                (byte)Math.Min(color.R * transformHighest, 255),
                (byte)Math.Min(color.G * transformHighest, 255),
                (byte)Math.Min(color.B * transformHighest, 255)
            );
        if (a >= b && a >= c) {
			if (b >= c) {
				return new Color(
					color.A,
					(byte)Math.Min(color.R * transformHighest, 255),
					(byte)Math.Min(color.G * transfomrMid, 255),
					(byte)Math.Min(color.B * transformLowest, 255)
				);
			} else {
				return new Color(
					color.A,
					(byte)Math.Min(color.R * transformHighest, 255),
					(byte)Math.Min(color.G * transformLowest, 255),
					(byte)Math.Min(color.B * transfomrMid, 255)
				);
			}
		} else if (b >= a && b >= c) {
			if (a >= c) {
				return new Color(
					color.A,
					(byte)Math.Min(color.R * transfomrMid, 255),
					(byte)Math.Min(color.G * transformHighest, 255),
					(byte)Math.Min(color.B * transformLowest, 255)
				);
			} else {
				return new Color(
					color.A,
					(byte)Math.Min(color.R * transformLowest, 255),
					(byte)Math.Min(color.G * transformHighest, 255),
					(byte)Math.Min(color.B * transfomrMid, 255)
				);
			}
		} else {
			if (a >= b) {
				return new Color(
					color.A,
					(byte)Math.Min(color.R * transfomrMid, 255),
					(byte)Math.Min(color.G * transformLowest, 255),
					(byte)Math.Min(color.B * transformHighest, 255)
				);
			} else {
				return new Color(
					color.A,
					(byte)Math.Min(color.R * transformLowest, 255),
					(byte)Math.Min(color.G * transfomrMid, 255),
					(byte)Math.Min(color.B * transformHighest, 255)
				);
			}
		}
	}

	/// <summary>
	/// Gets a Converter that takes several numbers as input and converts it into a text representation
	/// </summary>
	public static FuncMultiValueConverter<decimal?, string> MyMultiConverter {
		get;
	} =
		new FuncMultiValueConverter<decimal?, string>(num => $"Your numbers are: '{string.Join(", ", num)}'");
}

