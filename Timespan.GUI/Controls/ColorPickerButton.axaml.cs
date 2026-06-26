namespace Timespan.GUI.Controls;

using Avalonia.Interactivity;

using System;
using System.Collections.Generic;
using System.Text;

using Timespan.GUI.Types.Events;

public partial class ColorPickerButton : UserControl {
	// ── Styled Properties ────────────────────────────────────────────────────

	/// <summary>The currently selected color (bindable, default White).</summary>
	public static readonly StyledProperty<Color> SelectedColorProperty =
		AvaloniaProperty.Register<ColorPickerButton, Color>(
			nameof(SelectedColor),
			defaultValue: Colors.White);

	public Color SelectedColor {
		get => GetValue(SelectedColorProperty);
		set => SetValue(SelectedColorProperty, value);
	}

	// ── Routed Events ─────────────────────────────────────────────────────────

	/// <summary>Raised when the user clicks "Select" in the popup.</summary>
	public static readonly RoutedEvent<ColorSelectedEventArgs> ColorSelectedEvent =
		RoutedEvent.Register<ColorPickerButton, ColorSelectedEventArgs>(
			nameof(ColorSelected),
			RoutingStrategies.Bubble);

	public event EventHandler<ColorSelectedEventArgs>? ColorSelected {
		add => AddHandler(ColorSelectedEvent, value);
		remove => RemoveHandler(ColorSelectedEvent, value);
	}

	// ── Private state ─────────────────────────────────────────────────────────

	/// Tracks the in-progress color while the popup is open, so Cancel can revert.
	private Color _pendingColor;

	// ── Constructor ───────────────────────────────────────────────────────────

	public ColorPickerButton() {
		InitializeComponent();

		// Keep the hex label in sync whenever SelectedColor changes.
		this.GetObservable(SelectedColorProperty)
			.Subscribe(c => {
				if (PART_HexLabel is not null)
					PART_HexLabel.Text = ColorToHex(c);
			});
	}

	// ── Event handlers ────────────────────────────────────────────────────────

	private void OnPickerButtonClick(object? sender, RoutedEventArgs e) {
		_pendingColor = SelectedColor;

		// Sync picker to current color before opening.
		if (PART_ColorPicker is not null)
			PART_ColorPicker.Color = SelectedColor;

		PART_Popup.IsOpen = true;
	}

	private void OnColorChanged(object? sender, ColorChangedEventArgs e) {
		_pendingColor = e.NewColor;

		// Live-preview: update the swatch while browsing.
		if (PART_Swatch is not null)
			PART_Swatch.Background = new SolidColorBrush(_pendingColor);

		if (PART_HexLabel is not null)
			PART_HexLabel.Text = ColorToHex(_pendingColor);
	}

	private void OnSelectClick(object? sender, RoutedEventArgs e) {
		SelectedColor = _pendingColor;
		PART_Popup.IsOpen = false;

		RaiseEvent(new ColorSelectedEventArgs(ColorSelectedEvent, this, SelectedColor));
	}

	private void OnCancelClick(object? sender, RoutedEventArgs e) {
		// Revert the live-preview swatch.
		if (PART_Swatch is not null)
			PART_Swatch.Background = new SolidColorBrush(SelectedColor);

		if (PART_HexLabel is not null)
			PART_HexLabel.Text = ColorToHex(SelectedColor);

		PART_Popup.IsOpen = false;
	}

	// ── Helpers ───────────────────────────────────────────────────────────────

	private static string ColorToHex(Color c) =>
		$"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
}