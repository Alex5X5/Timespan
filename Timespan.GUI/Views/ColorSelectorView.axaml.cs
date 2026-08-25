namespace Timespan.GUI.Views;

using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;

using Microsoft.Extensions.DependencyInjection;

using Timespan.GUI.Generators.Attributes;
using Timespan.GUI.Popups;
using Timespan.GUI.Services;
using Timespan.GUI.ViewModels;

public partial class ColorSelectorView : UserControl {

	private GuiStateService stateService;


	private ColorPickerPopup picker;

	#region styled properties

	[BasicStyledProperty<ColorSelectorView>]
	private IRelayCommand loadCommand;
	[BasicStyledProperty<ColorSelectorView>]
	private IRelayCommand unloadCommand;

	[BasicDirectProperty<ColorSelectorView>]
	private bool showColorPickerPopup;

	[BasicDirectProperty<ColorSelectorView>]
	private Color pickedColor = Colors.White;

	#endregion

	public ColorSelectorView() {
		InitializeComponent();
		var vm = App.Current.Services.GetService<ColorSelectorViewModel>();
		DataContext = vm;
		Bind(LoadCommandProperty, new Binding(nameof(vm.LoadCommand)) { Source = vm });
		Bind(UnloadCommandProperty, new Binding(nameof(vm.UnloadCommand)) { Source = vm });
		Bind(PickedColorProperty, new Binding(nameof(vm.PickedColor)) { Source = vm, Mode = BindingMode.TwoWay });
		Bind(ShowColorPickerPopupProperty, new Binding(nameof(vm.ShowColorPicker)) { Source = vm, Mode = BindingMode.TwoWay });

		AddHandler(LoadedEvent, OnLoad);
		AddHandler(UnloadedEvent, OnUnload);
	}

	private void OnLoad(object? sender, RoutedEventArgs args) {
		LoadCommand.Execute(EventArgs.Empty);
	}

	private void OnUnload(object? sender, RoutedEventArgs args) {
		UnloadCommand.Execute(EventArgs.Empty);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
		base.OnPropertyChanged(change);
		if (change.Property == ShowColorPickerPopupProperty) {
			var setOpen = (change.NewValue as bool?) ?? false;
			var isOpen = picker?.IsActive ?? false;
			if (setOpen && !isOpen) {
				OpenPicker();
			} else if (!setOpen && isOpen) {
				picker.Close();
			}
		}
	}

	private void OpenPicker() {
		if (TopLevel.GetTopLevel(this) is Window window) {
			picker = new ColorPickerPopup() { Color = PickedColor };
			picker.Closing += PickerClosingCallback;
			picker.ShowDialog(window);
		}
	}

	private void PickerClosingCallback(object? sender, WindowClosingEventArgs e) {
		PickedColor = picker.Color;
		(DataContext as ColorSelectorViewModel)
			?.ColorPickedCommand.Execute(EventArgs.Empty);
	}
}