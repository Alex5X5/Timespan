using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;

namespace Timespan.GUI.ViewModels;

public partial class TaskDetailsViewModel : ViewModelBase {

	private IHourglassDbService dbService;
	private Timespan.GUI.Services.CacheService cacheService;

	[ObservableProperty]
	private bool showTaskPanel = true;

	[ObservableProperty]
	private bool showReadonlyTaskPanel = true;

	[ObservableProperty]
	private bool showEditTaskPanel = false;

	[ObservableProperty]
	private string showingTaskTitle = "a title";

	[ObservableProperty]
	private string showingTaskDescription = "lorem ipsum dolor sit amet condecteter";

	[ObservableProperty]
	private string showingTaskDateString = "Mi. 18. Feb. 2026";

	[ObservableProperty]
	private string showingTaskTimeString = "07:34 - 11:53";


	[RelayCommand]
	internal async Task DeleteTask() {
		await dbService.DeleteTaskAsync(cacheService.SelectedTask)
			.ContinueWith(
				(state) =>
					GlobalEventService.Raise<TasksChangedEventArgs>());
	}

	[RelayCommand]
	internal void EditTask() {
		Console.WriteLine("[GraphsView]: editing task");
		ShowEditTaskPanel = true;
		ShowReadonlyTaskPanel = false;

	}

	[RelayCommand]
	internal void ApplyEdit() {
		Console.WriteLine("[GraphsView]: editing task");
		ShowEditTaskPanel = true;
		ShowReadonlyTaskPanel = false;

	}

	[RelayCommand]
	internal void CanelEdit() {
		Console.WriteLine("[GraphsView]: editing task");
		ShowEditTaskPanel = true;
		ShowReadonlyTaskPanel = false;

	}

}
