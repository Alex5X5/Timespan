namespace Timespan.GUI.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;

using Timespan.Database.Services.Interfaces;
using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;
using Timespan.GUI.ViewModels.Graphs;
using Timespan.Util.Services;

public partial class GraphsViewModel : ViewModelBase, IMainViewChild {

	private IHourglassDbService dbService;

	public RedirectionAnchor<IGraphsViewChild> CurrentPageAnchor;
	public IGraphsViewChild? CurrentPage => CurrentPageAnchor.CurrentModel;

	private string selectedItem = "";
	public string SelectedItem {
		set {
			selectedItem = value;
			UpdateMode(value);
			OnPropertyChanged(nameof(SelectedItem));
			OnPropertyChanged(nameof(DateString));
		}
		get => selectedItem;
	}

	public ObservableCollection<string> Items { get; }
	
	public string DateString => CurrentPage?.GetDateString() ?? "Date";

	[ObservableProperty]
	private bool showTaskPanel = false;

    [ObservableProperty]
    private string showingTaskTitle = "a title";

    [ObservableProperty]
    private string showingTaskDescription = "lorem ipsum dolor sit amet condecteter";

    [ObservableProperty]
    private string showingTaskDateString = "Mi. 18. Feb. 2026";

    [ObservableProperty]
    private string showingTaskTimeString = "07:34 - 11:53";

    partial void OnShowTaskPanelChanged(bool value)
	{
		SpacerWidth = new(value ? 2 : 0, GridUnitType.Star);
		TaskPanelWidth = new(value ? 24 : 0, GridUnitType.Star);
	}

	[ObservableProperty]
	private GridLength spacerWidth = new(0, GridUnitType.Star);

	[ObservableProperty]
	private GridLength taskPanelWidth = new(0, GridUnitType.Star);

	public GraphsViewModel(RedirectionService redirectionService, ViewModelFactory<IGraphsViewChild> factory, IHourglassDbService dbService) : base() {
		this.dbService = dbService;
		CurrentPageAnchor = new(factory);
		redirectionService.Register<GraphsViewModel, IGraphsViewChild>(CurrentPageAnchor);
		CurrentPageAnchor.ModelChanged += () => {
			OnPropertyChanged(nameof(CurrentPage));
			OnPropertyChanged(nameof(DateString));
		};
		Items = new() { "Day", "Week", "Month" };
		SelectedItem = Items[0];
		if(GlobalEventService.GetEvent<ShowTaksEventArgs>() is EventDispatcher<ShowTaksEventArgs> dispatcher)
			dispatcher += ShowTask;
	}

	[RelayCommand]
	internal void ShowTask(ShowTaksEventArgs args) {
		Console.WriteLine("[GraphsView]:showing task");
		if (args.Task is null) {
			HideTask();
		} else {
			ShowTaskPanel = true;
			ShowingTaskDescription = args.Task.description;
			string day = args.Task.StartDateTime.DayOfWeek switch {
				DayOfWeek.Monday => TranslatorService.Singleton["Days.Short.Monday"] ?? "Mo",
				DayOfWeek.Tuesday => TranslatorService.Singleton["Days.Short.Tuesday"] ?? "Tu",
				DayOfWeek.Wednesday => TranslatorService.Singleton["Days.Short.Wednesday"] ?? "We",
				DayOfWeek.Thursday => TranslatorService.Singleton["Days.Short.Thursday"] ?? "Th",
				DayOfWeek.Friday => TranslatorService.Singleton["Days.Short.Friday"] ?? "Fr",
				DayOfWeek.Saturday => TranslatorService.Singleton["Days.Short.Saturday"] ?? "Sa",
				DayOfWeek.Sunday => TranslatorService.Singleton["Days.Short.Sunday"] ?? "Su",
				_ => ""
			};

			string month = args.Task.StartDateTime.Month switch {
				1 => TranslatorService.Singleton["Months.January"] ?? "January",
				2 => TranslatorService.Singleton["Days.Short.February"] ?? "February",
				3 => TranslatorService.Singleton["Days.Short.March"] ?? "March",
				4 => TranslatorService.Singleton["Days.Short.April"] ?? "April",
				5 => TranslatorService.Singleton["Days.Short.May"] ?? "May",
				6 => TranslatorService.Singleton["Days.Short.June"] ?? "June",
				7 => TranslatorService.Singleton["Days.Short.July"] ?? "July",
				8 => TranslatorService.Singleton["Days.Short.August"] ?? "August",
				9 => TranslatorService.Singleton["Days.Short.September"] ?? "September",
				10 => TranslatorService.Singleton["Days.Short.October"] ?? "October",
				11 => TranslatorService.Singleton["Days.Short.November"] ?? "November",
				12 => TranslatorService.Singleton["Days.Short.December"] ?? "December",
				_ => ""
			};

			ShowingTaskDateString = $"{day}. {args.Task.StartDateTime.Day}. {month}. {args.Task.StartDateTime.Year}";
			
		}
	}

	[RelayCommand]
	internal void HideTask() {
		Console.WriteLine("[GraphsView]:hiding task");
		ShowTaskPanel = false;
	}

	[RelayCommand]
	internal void DeleteTask() {
		HideTask();
	}

	[RelayCommand]
	internal void EditTask() {
		
	}

	[RelayCommand]
	internal void Select() {
		if (dbService.QueryCurrentTaskAsync().Result is Timespan.Types.Models.Task task) {
			var args = new ShowTaksEventArgs(task);
			GlobalEventService.Raise(args);
		}
	}

	[RelayCommand]
	internal void Delete() {
		var args = new ShowTaksEventArgs();
		GlobalEventService.Raise(args);
	}

	private void UpdateMode(string mode) {
		if(mode == Items[0])
			CurrentPageAnchor.ChangeModel<DayViewModel>();
		if (mode == Items[1])
			CurrentPageAnchor.ChangeModel<WeekViewModel>();
		if (mode == Items[2])
			CurrentPageAnchor.ChangeModel<MonthViewModel>();
	}

	internal void OnLoad() {
		CurrentPageAnchor.ChangeModel<DayViewModel>();
	}
}
