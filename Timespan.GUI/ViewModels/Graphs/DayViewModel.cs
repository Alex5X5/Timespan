using CommunityToolkit.Mvvm.ComponentModel;

using System.Threading.Tasks;

using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;

namespace Timespan.GUI.ViewModels.Graphs;

public partial class DayViewModel : ViewModelBase, IGraphsViewChild {

	public GUI.Services.CacheService CacheService;	

    [ObservableProperty]
    private GridLength[] columnWidths = new[]
    {
        new GridLength(1, GridUnitType.Star),
        new GridLength(1, GridUnitType.Pixel)
    };

	public bool[] MarkedColumns;
	public bool[] BlockedColumns;

	public DayViewModel() : this(null) {
		MarkedColumns = new bool[32];
		BlockedColumns = new bool[32];
		for (int i = 0; i < MarkedColumns.Length; i++) {
			MarkedColumns[i] = false;
		}
	}

    public DayViewModel(GUI.Services.CacheService cacheService) : base() {
		CacheService = cacheService;
		//for(int i=0; i<showColumn.Length; i++) {
		//	if (i < 6) { 
		//		showColumn[i] = new(0, GridUnitType.Star);
		//	} else if(i > 17) {
		//		showColumn[i] = new(0, GridUnitType.Star);
		//	} else {
		//		showColumn[i] = new(1, GridUnitType.Star);
		//	}
		//};
	}

	public string GetDateString() {
		return "Montag 14.3.";
	}

	public async Task GetTasksAsync() {

	}

	public async Task OnMissingContextMenuClicked(Timespan.Types.Models.BlockedTimeIntervallType reason) {
		await SetTimeIntervallBlocked(reason);
	}


	public async Task SetTimeIntervallBlocked(Timespan.Types.Models.BlockedTimeIntervallType reason) {
	}

	public async Task SetTimeIntervallUnblocked() {
	}

	public virtual void OnTaskClicked(Timespan.Types.Models.Task task) {
		GlobalEventService.Raise(new ShowTaksEventArgs(task));
	}
}
