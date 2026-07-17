namespace Timespan.GUI.Popups;

using Timespan.GUI.Generators.Attributes;

public partial class ColorPickerPopup : Window {
	
	[BasicStyledProperty<ColorPickerPopup>]
	private Color color;

	public ColorPickerPopup() {
		InitializeComponent();
	}
}