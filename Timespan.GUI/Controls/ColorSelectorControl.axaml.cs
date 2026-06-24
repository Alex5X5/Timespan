namespace Timespan.GUI.Controls;

using Avalonia.Interactivity;
using Avalonia.Media;

using CommunityToolkit.Mvvm.Input;

using Timespan.GUI.Generators.Attributes;
using Timespan.GUI.Services;
using Timespan.GUI.Services.Mapping;
using Timespan.GUI.Types.Events;

public partial class ColorSelectorControl : UserControl {

	#region styled properties

	public static readonly StyledProperty<IRelayCommand<Avalonia.Media.Color>> ColorChangedCommandProperty =
		AvaloniaProperty.Register<ColorSelectorControl, IRelayCommand<Avalonia.Media.Color>>(nameof(ColorChangedCommand), new RelayCommand<Avalonia.Media.Color>(color => { }));

	public IRelayCommand<Avalonia.Media.Color> ColorChangedCommand {
		get => GetValue(ColorChangedCommandProperty);
		set => SetValue(ColorChangedCommandProperty, value);
	}

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
		Color fallback = new Color(255, 79, 79, 79);
		Color color = ((sender as Button)?.Background as SolidColorBrush)?.Color ?? fallback;
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
		GlobalEventService.Subscribe<SelectedtaskChangedEventArgs>(ShowTask);
		InvalidateVisual();
	}

	private void OnUnload(object? sender, RoutedEventArgs args) {
		GlobalEventService.UnSubscribe<ShowTaksEventArgs>(ShowTask);
		GlobalEventService.UnSubscribe<SelectedtaskChangedEventArgs>(ShowTask);
	}

	private void ShowTask(ShowTaksEventArgs args) {
		if (args.Task == null)
			return;
		foreach(var button in buttons)
			if (TaskHasColorAsButton(button, args.Task))
				SetButtonSelected(button);
	}

	private static bool TaskHasColorAsButton(Button button, Timespan.Types.Models.Task? task) {
		if(task == null)
			return false;
		return (button.Background as SolidColorBrush)?.Color == task.DisplayColor;
	}
}