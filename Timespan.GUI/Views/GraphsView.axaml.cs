using Avalonia.Interactivity;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

using Timespan.GUI.ViewModels;
using Timespan.GUI.Views.Graphs;
using Timespan.Util.Attributes;
using Timespan.Util.Services;

namespace Timespan.GUI.Views;

internal partial class GraphsView : UserControl {

	[TranslateMember("Views.Pages.Graphs.Labels.Title", "Graphs")]
	public string TitleLabelText { get; set; } = "";
	[TranslateMember("Views.Pages.Graphs.Buttons.Delete", "Delete")]
	public string DeleteButtonText { get; set; } = "";
	[TranslateMember("Views.Pages.Graphs.Buttons.Select", "Select")]
	public string SelectButtonText { get; set; } = "";


	public static readonly StyledProperty<IRelayCommand> LoadCommandProperty =
		AvaloniaProperty.Register<GraphsView, IRelayCommand>(nameof(LoadCommand), new RelayCommand(() => { }));

	public static readonly StyledProperty<IRelayCommand> UnloadCommandProperty =
		AvaloniaProperty.Register<GraphsView, IRelayCommand>(nameof(UnloadCommand), new RelayCommand(() => { }));
	

	public IRelayCommand LoadCommand {
		get => GetValue(LoadCommandProperty);
		set => SetValue(LoadCommandProperty, value);
	}

	public IRelayCommand UnloadCommand {
		get => GetValue(UnloadCommandProperty);
		set => SetValue(UnloadCommandProperty, value);
	}


	public GraphsView() {
		TranslatorService.Singleton.TranslateAnnotatedMembers(this);
        InitializeComponent();
		AddHandler(LoadedEvent, OnLoad);
		AddHandler(UnloadedEvent, OnUnload);
		this.DataContext = App.Current.Services.GetService<GraphsViewModel>();
	}

	private void OnLoad(object? sender, RoutedEventArgs args) {
		LoadCommand.Execute(EventArgs.Empty);
	}

	private void OnUnload(object? sender, RoutedEventArgs args) {
		UnloadCommand.Execute(EventArgs.Empty);
	}
}