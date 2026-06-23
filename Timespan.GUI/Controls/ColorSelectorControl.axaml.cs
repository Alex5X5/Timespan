namespace Timespan.GUI.Controls;

using Avalonia.Interactivity;
using Avalonia.Media;

using CommunityToolkit.Mvvm.Input;

using Timespan.GUI.Services;
using Timespan.GUI.Services.Mapping;
using Timespan.GUI.Types.Events;
using Timespan.GUI.ViewModels;
using Timespan.Util.Services;

internal partial class ColorSelectorControl : UserControl {

	#region styled properties

	public static readonly StyledProperty<IRelayCommand<Avalonia.Media.Color>> ColorChangedCommandProperty =
		AvaloniaProperty.Register<ColorSelectorControl, IRelayCommand<Avalonia.Media.Color>>(nameof(ColorChangedCommand), new RelayCommand<Avalonia.Media.Color>(color => { }));

	public static readonly DirectProperty<TaskDetailsControl, bool> ShowReadonlyTaskPanelProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, bool>(
			nameof(ShowReadonlyTaskPanel),
			control => control.ShowReadonlyTaskPanel,
			(control, value) => control.ShowReadonlyTaskPanel = value,
			true,
			Avalonia.Data.BindingMode.TwoWay);

	public static readonly DirectProperty<TaskDetailsControl, bool> Color1ButtonSelectedProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, bool>(
			nameof(Color1ButtonSelected),
			control => control.Color1ButtonSelected,
			(control, value) => control.Color1ButtonSelected = value,
			true,
			Avalonia.Data.BindingMode.TwoWay);

	public static readonly DirectProperty<TaskDetailsControl, bool> Color2ButtonSelectedProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, bool>(
			nameof(Color2ButtonSelected),
			control => control.Color2ButtonSelected,
			(control, value) => control.Color2ButtonSelected = value,
			true,
			Avalonia.Data.BindingMode.TwoWay);

	public static readonly DirectProperty<TaskDetailsControl, bool> Color3ButtonSelectedProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, bool>(
			nameof(Color3ButtonSelected),
			control => control.Color3ButtonSelected,
			(control, value) => control.Color3ButtonSelected = value,
			true,
			Avalonia.Data.BindingMode.TwoWay);

	public static readonly DirectProperty<TaskDetailsControl, bool> Color4ButtonSelectedProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, bool>(
			nameof(Color4ButtonSelected),
			control => control.Color4ButtonSelected,
			(control, value) => control.Color4ButtonSelected = value,
			true,
			Avalonia.Data.BindingMode.TwoWay);

	public static readonly DirectProperty<TaskDetailsControl, bool> Color5ButtonSelectedProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, bool>(
			nameof(Color5ButtonSelected),
			control => control.Color5ButtonSelected,
			(control, value) => control.Color5ButtonSelected = value,
			true,
			Avalonia.Data.BindingMode.TwoWay);

	public static readonly DirectProperty<TaskDetailsControl, bool> Color6ButtonSelectedProperty =
		AvaloniaProperty.RegisterDirect<TaskDetailsControl, bool>(
			nameof(Color6ButtonSelected),
			obj => obj.Color6ButtonSelected,
			(obj, value) => obj.Color6ButtonSelected = value,
			true,
			Avalonia.Data.BindingMode.TwoWay);

	public IRelayCommand<Avalonia.Media.Color> ColorChangedCommand {
		get => GetValue(ColorChangedCommandProperty);
		set => SetValue(ColorChangedCommandProperty, value);
	}

	public bool ShowReadonlyTaskPanel {
		get => showReadonlyTaskPanel;
		set => SetAndRaise(ShowReadonlyTaskPanelProperty, ref showReadonlyTaskPanel, value);
	}
	private bool showReadonlyTaskPanel = true;

	public bool Color1ButtonSelected {
		get => color1ButtonSelected;
		set => SetAndRaise(Color1ButtonSelectedProperty, ref color1ButtonSelected, value);
	};
	private bool color1ButtonSelected = true;

	public bool Color2ButtonSelected {
		get => color2ButtonSelected;
		set => SetAndRaise(Color2ButtonSelectedProperty, ref color2ButtonSelected, value);
	};
	private bool color2ButtonSelected = false;
	public bool Color3ButtonSelected {
		get => color3ButtonSelected;
		set => SetAndRaise(Color3ButtonSelectedProperty, ref color3ButtonSelected, value);
	};
	private bool color3ButtonSelected = false;
	public bool Color4ButtonSelected {
		get => color4ButtonSelected;
		set => SetAndRaise(Color4ButtonSelectedProperty, ref color4ButtonSelected, value);
	};
	private bool color4ButtonSelected = false;
	public bool Color5ButtonSelected {
		get => color5ButtonSelected;
		set => SetAndRaise(Color5ButtonSelectedProperty, ref color5ButtonSelected, value);
	};
	private bool color5ButtonSelected = false;
	public bool Color6ButtonSelected {
		get => color6ButtonSelected;
		set => SetAndRaise(Color6ButtonSelectedProperty, ref color6ButtonSelected, value);
	};
	private bool color6ButtonSelected = false;

	#endregion

	public ColorSelectorControl() {
		InitializeComponent();
		//AddHandler(LoadedEvent, OnLoad);
		//AddHandler(UnloadedEvent, OnUnload);
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
		OnPropertyChanged();
		OnPropertyChanged(nameof(Color2ButtonSelected));
		OnPropertyChanged(nameof(Color3ButtonSelected));
		OnPropertyChanged(nameof(Color4ButtonSelected));
		OnPropertyChanged(nameof(Color5ButtonSelected));
		OnPropertyChanged(nameof(Color6ButtonSelected));
	}

	public void PickerButtonClick(object sender, RoutedEventArgs e) {
	}

	//private void OnLoad(object? sender, RoutedEventArgs args) {
	//}

	//private void OnUnload(object? sender, RoutedEventArgs args) {
	//}
}