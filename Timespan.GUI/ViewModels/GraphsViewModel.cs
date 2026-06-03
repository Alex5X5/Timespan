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

	private string selectedTimeMode = "";
	public string SelectedTimeMode {
		set {
			selectedTimeMode = value;
			UpdateMode(value);
			OnPropertyChanged(nameof(SelectedTimeMode));
			OnPropertyChanged(nameof(CurrentPage));
			OnPropertyChanged(nameof(DateString));
		}
		get => selectedTimeMode;
	}

	public ObservableCollection<string> TimeModes { get; }
	
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
		SpacerWidth = new(value ? 1 : 0, GridUnitType.Star);
		TaskPanelWidth = new(value ? 19 : 0, GridUnitType.Star);
	}

	[ObservableProperty]
	private GridLength spacerWidth = new(0, GridUnitType.Star);

	[ObservableProperty]
	private GridLength taskPanelWidth = new(0, GridUnitType.Star);

	public GraphsViewModel(RedirectionService redirectionService, ViewModelFactory<IGraphsViewChild> factory, IHourglassDbService dbService) : base() {
		this.dbService = dbService;
		CurrentPageAnchor = new(factory);
		redirectionService.Register<GraphsViewModel, IGraphsViewChild>(CurrentPageAnchor);
		CurrentPageAnchor.ModelChanged += (from, to) => {
			OnPropertyChanged(nameof(CurrentPage));
			OnPropertyChanged(nameof(DateString));
		};
		TimeModes = new() { "Day", "Week", "Month" };
		SelectedTimeMode = TimeModes[0];
		if(GlobalEventService.GetEvent<ShowTaksEventArgs>() is EventDispatcher<ShowTaksEventArgs> dispatcher)
			dispatcher += ShowTask;
		CurrentPageAnchor.ChangeModel<DayViewModel>();
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
				2 => TranslatorService.Singleton["Months.February"] ?? "February",
				3 => TranslatorService.Singleton["Months.March"] ?? "March",
				4 => TranslatorService.Singleton["Months.April"] ?? "April",
				5 => TranslatorService.Singleton["Months.May"] ?? "May",
				6 => TranslatorService.Singleton["Months.June"] ?? "June",
				7 => TranslatorService.Singleton["Months.July"] ?? "July",
				8 => TranslatorService.Singleton["Months.August"] ?? "August",
				9 => TranslatorService.Singleton["Months.September"] ?? "September",
				10 => TranslatorService.Singleton["Months.October"] ?? "October",
				11 => TranslatorService.Singleton["Months.November"] ?? "November",
				12 => TranslatorService.Singleton["Months.December"] ?? "December",
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
		if(mode == TimeModes[0])
			CurrentPageAnchor.ChangeModel<DayViewModel>();
		if (mode == TimeModes[1])
			CurrentPageAnchor.ChangeModel<WeekViewModel>();
		if (mode == TimeModes[2])
			CurrentPageAnchor.ChangeModel<MonthViewModel>();
	}

	internal void OnLoad() {
	}
}
