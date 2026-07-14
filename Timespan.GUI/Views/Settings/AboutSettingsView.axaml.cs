using Avalonia.Data;

using Timespan.GUI.Generators.Attributes;
using Timespan.GUI.ViewModels.Settings;
using Timespan.Util.Attributes;
using Timespan.Util.Services;

namespace Timespan.GUI.Views.Settings;

public partial class AboutSettingsView : UserControl {

	[TranslateMember("Views.Pages.Settings.About.Labels.Title", "About")]
	public string TitleLabelText { get; set; } = "";

	public AboutSettingsView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
		InitializeComponent();
	}

	private void TextClick(object? sender, Avalonia.Input.PointerPressedEventArgs e) {
		if (sender == EmailButton) {
			AboutSettingsViewModel.OnEmailButtonClick();
		} else if (sender == SrhButton) {
			AboutSettingsViewModel.OnSrhButtonClick();
		} else if (sender == DotnetButton) {
			AboutSettingsViewModel.OnDotnetButtonClick();
		} else if (sender == AvaloniaButton) {
			AboutSettingsViewModel.OnAvaloniaButtonClick();
		} else if (sender == VisualStudioButton) {
			AboutSettingsViewModel.OnVisualStudioButtonClick();
		} else if (sender == FigmaButton) {
			AboutSettingsViewModel.OnFigmaButtonClick();
		} else if (sender == IllustratorButton) {
			AboutSettingsViewModel.OnIllustratorButtonClick();
		} else if (sender == GithubButton) {
			AboutSettingsViewModel.OnGithubButtonClick();
		} else if (sender == ProgrammButton) {
			AboutSettingsViewModel.OnGithubButtonClick();
		} else if (sender == KofiButton) {
			AboutSettingsViewModel.OnKofiButtonClick();
		}
	}
}