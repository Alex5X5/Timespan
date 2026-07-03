namespace Timespan.GUI.Views;

using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

using Timespan.GUI.Generators.Attributes;
using Timespan.GUI.ViewModels;

public partial class TaskDetailsView : UserControl {
	
	[BasicStyledProperty<TaskDetailsView>]
	private IRelayCommand loadCommand;
	[BasicStyledProperty<TaskDetailsView>]
	private IRelayCommand unloadCommand;


	public TaskDetailsView() {
		var vm = App.Current.Services.GetService<TaskDetailsViewModel>();
		DataContext = vm;
		this.Bind(LoadCommandProperty, new Binding(nameof(vm.LoadCommand)) { Source = vm });
		this.Bind(UnloadCommandProperty, new Binding(nameof(vm.UnloadCommand)) { Source = vm });
		InitializeComponent();
		AddHandler(LoadedEvent, OnLoad);
		AddHandler(UnloadedEvent, OnUnload);
	}

	private void OnLoad(object? sender, RoutedEventArgs args) {
		if (LoadCommand?.CanExecute(EventArgs.Empty) ?? false)
			LoadCommand.Execute(EventArgs.Empty);
	}

	private void OnUnload(object? sender, RoutedEventArgs args) {
		if (UnloadCommand?.CanExecute(EventArgs.Empty) ?? false)
			UnloadCommand.Execute(EventArgs.Empty);
	}
}