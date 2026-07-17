namespace Timespan.GUI.ViewModels;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;

using Timespan.Database.Services;
using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;

public partial class ColorSelectorViewModel : ViewModelBase {

	private GuiStateService stateService;
	private ColorService colorService;
	private ITimespanDbService dbService;

	#region Observable Properties

	[ObservableProperty]
	private bool color1ButtonSelected = false;
	[ObservableProperty]
	private bool color2ButtonSelected = false;
	[ObservableProperty]
	private bool color3ButtonSelected = false;
	[ObservableProperty]
	private bool color4ButtonSelected = false;
	[ObservableProperty]
	private bool color5ButtonSelected = false;
	[ObservableProperty]
	private bool color6ButtonSelected = false;
	[ObservableProperty]
	private bool color7ButtonSelected = false;

	[ObservableProperty]
	private Color button1Color = Colors.White;
	[ObservableProperty]
	private Color button2Color = Colors.White;
	[ObservableProperty]
	private Color button3Color = Colors.White;
	[ObservableProperty]
	private Color button4Color = Colors.White;
	[ObservableProperty]
	private Color button5Color = Colors.White;
	[ObservableProperty]
	private Color button6Color = Colors.White;
	[ObservableProperty]
	private Color pickedColor = Colors.White;

	[ObservableProperty]
	private bool showColorPicker = false;

	#endregion

	public ColorSelectorViewModel() {
		stateService = App.Current.Services.GetRequiredService<GuiStateService>();
		colorService = App.Current.Services.GetRequiredService<ColorService>();
		dbService = App.Current.Services.GetRequiredService<ITimespanDbService>();
		Button1Color = colorService.TASK_BACKGROUND_RED;
		Button2Color = colorService.TASK_BACKGROUND_ORANGE;
		Button3Color = colorService.TASK_BACKGROUND_YELLOW;
		Button4Color = colorService.TASK_BACKGROUND_DARK_GREEN;
		Button5Color = colorService.TASK_BACKGROUND_DARK_BLUE;
		Button6Color = colorService.TASK_BACKGROUND_DARK_PURPLE;
	}

	#region button commands

	[RelayCommand]
	private void Color1ButtonClick() {
		Color1ButtonSelected = true;
		Color2ButtonSelected = false;
		Color3ButtonSelected = false;
		Color4ButtonSelected = false;
		Color5ButtonSelected = false;
		Color6ButtonSelected = false;
		Color7ButtonSelected = false;
		UpdateSelectedColor(Button1Color);
	}

	[RelayCommand]
	private void Color2ButtonClick() {
		Color1ButtonSelected = false;
		Color2ButtonSelected = true;
		Color3ButtonSelected = false;
		Color4ButtonSelected = false;
		Color5ButtonSelected = false;
		Color6ButtonSelected = false;
		Color7ButtonSelected = false;
		UpdateSelectedColor(Button2Color);
	}

	[RelayCommand]
	private void Color3ButtonClick() {
		Color1ButtonSelected = false;
		Color2ButtonSelected = false;
		Color3ButtonSelected = true;
		Color4ButtonSelected = false;
		Color5ButtonSelected = false;
		Color6ButtonSelected = false;
		Color7ButtonSelected = false;
		UpdateSelectedColor(Button3Color);
	}

	[RelayCommand]
	private void Color4ButtonClick() {
		Color1ButtonSelected = false;
		Color2ButtonSelected = false;
		Color3ButtonSelected = false;
		Color4ButtonSelected = true;
		Color5ButtonSelected = false;
		Color6ButtonSelected = false;
		Color7ButtonSelected = false;
		UpdateSelectedColor(Button4Color);
	}

	[RelayCommand]
	private void Color5ButtonClick() {
		Color1ButtonSelected = false;
		Color2ButtonSelected = false;
		Color3ButtonSelected = false;
		Color4ButtonSelected = false;
		Color5ButtonSelected = true;
		Color6ButtonSelected = false;
		Color7ButtonSelected = false;
		UpdateSelectedColor(Button5Color);
	}

	[RelayCommand]
	private void Color6ButtonClick() {
		Color1ButtonSelected = false;
		Color2ButtonSelected = false;
		Color3ButtonSelected = false;
		Color4ButtonSelected = false;
		Color5ButtonSelected = false;
		Color6ButtonSelected = true;
		Color7ButtonSelected = false;
		UpdateSelectedColor(Button6Color);
	}
	
	[RelayCommand]
	private void PickerButtonClick() {
		Color1ButtonSelected = false;
		Color2ButtonSelected = false;
		Color3ButtonSelected = false;
		Color4ButtonSelected = false;
		Color5ButtonSelected = false;
		Color6ButtonSelected = false;
		if (Color7ButtonSelected) {
			ShowColorPicker = !ShowColorPicker;
		} else {
			Color7ButtonSelected = true;
		}
		UpdateSelectedColor(PickedColor);
	}

	[RelayCommand]
	private void UpdateSelectedColor(Color color) {
		stateService.SelectedColor = color;
		GlobalEventService.Raise<ColorSelectedEventArgs>();
	}

	#endregion

	#region events

	[RelayCommand]
	private void OnLoad() {
		GlobalEventService.Subscribe<ShowTaksEventArgs>(OnShowTask);
		OnTaskChanged();
	}

	[RelayCommand]
	private void OnUnload() {
		GlobalEventService.UnSubscribe<ShowTaksEventArgs>(OnShowTask);
	}


	[RelayCommand]
	private void OnColorPicked() {
		UpdateSelectedColor(PickedColor);
	}

	private void OnShowTask(ShowTaksEventArgs args) {
		OnTaskChanged();
	}

	private void OnTaskChanged() {
		if (stateService.SelectedColor == Button1Color) {
			Color1ButtonSelected = true;
		} else if (stateService.SelectedColor == Button2Color) {
			Color2ButtonSelected = true;
		} else if (stateService.SelectedColor == Button3Color) {
			Color3ButtonSelected = true;
		} else if (stateService.SelectedColor == Button4Color) {
			Color4ButtonSelected = true;
		} else if (stateService.SelectedColor == Button5Color) {
			Color5ButtonSelected = true;
		} else if (stateService.SelectedColor == Button6Color) {
			Color6ButtonSelected = true;
		} else {
			Color7ButtonSelected = true;
			PickedColor = stateService.SelectedColor;
		}
	}

	#endregion
}