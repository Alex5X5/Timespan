namespace Timespan.GUI.Controls;

using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;

using Timespan.GUI.Generators.Attributes;
using Timespan.GUI.Services;
using Timespan.GUI.Services.Mapping;
using Timespan.GUI.Types;
using Timespan.GUI.Types.Events;

public partial class ColorSelectorControl : UserControl {

	#region styled properties

	[BasicStyledProperty<ColorSelectorControl>]
	private IRelayCommand<Color> colorChangedCommand;

	[BasicStyledProperty<ColorSelectorControl>]
	private ObservableTask selectedTask;

	[BasicStyledProperty<ColorSelectorControl>]
	private Color selectedColor = Color.FromArgb(255, 70, 70, 70);

	[BasicDirectProperty<ColorSelectorControl>]
	private bool color1ButtonSelected = false;

	[BasicDirectProperty<ColorSelectorControl>]
	private bool color2ButtonSelected = false;

	[BasicDirectProperty<ColorSelectorControl>]
	private bool color3ButtonSelected = false;

	[BasicDirectProperty<ColorSelectorControl>]
	private bool color4ButtonSelected = false;

	[BasicDirectProperty<ColorSelectorControl>]
	private bool color5ButtonSelected = false;

	[BasicDirectProperty<ColorSelectorControl>]
	private bool color6ButtonSelected = false;

	#endregion

	private readonly Button[] buttons;

	public ColorSelectorControl() {
		InitializeComponent();
		buttons = [
			Color1Button,
			Color2Button,
			Color3Button,
			Color4Button,
			Color5Button,
			Color6Button
		];
		AddHandler(LoadedEvent, OnLoad);
		AddHandler(UnloadedEvent, OnUnload);
	}

	public void ColorButtonClick(object sender, RoutedEventArgs e) {
		Color color = GetButtonBackground(sender as Button);
		SetButtonSelected(sender);
		if (ColorChangedCommand.CanExecute(color))
			ColorChangedCommand.Execute(color);
	}

	private void SetButtonSelected(object sender) {
		Color1ButtonSelected = sender == Color1Button;
		Color2ButtonSelected = sender == Color2Button;
		Color3ButtonSelected = sender == Color3Button;
		Color4ButtonSelected = sender == Color4Button;
		Color5ButtonSelected = sender == Color5Button;
		Color6ButtonSelected = sender == Color6Button;
	}

	public void PickerButtonClick(object sender, RoutedEventArgs e) {
		
	}

	private void OnLoad(object? sender, RoutedEventArgs args) {
		GlobalEventService.Subscribe<ShowTaksEventArgs>(ShowTask);
		OnTaskChanged();
	}

	private void OnUnload(object? sender, RoutedEventArgs args) {
		GlobalEventService.UnSubscribe<ShowTaksEventArgs>(ShowTask);
	}

	private void ShowTask(ShowTaksEventArgs args) {
		OnTaskChanged();
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
		base.OnPropertyChanged(change);
		if (change.Property == SelectedTaskProperty) {
			if (SelectedTask != null)
				OnTaskChanged();
		}
	}

	private void OnTaskChanged() {
		if (SelectedTask != null) {
			SelectedColor = SelectedTask.DisplayColor;
			foreach (var button in buttons) {
				if (GetButtonBackground(button) == SelectedColor)
					SetButtonSelected(button);
			}
		}
	}

	private static Color GetButtonBackground(Button? button) {
		return (button?.Background as ImmutableSolidColorBrush)?.Color ?? new(255, 79, 79, 79);
	}
}