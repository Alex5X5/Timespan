using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Timespan.GUI.ViewModels.Graphs;

public partial class DayViewModel : ViewModelBase, IGraphsViewChild {


	public ObservableCollection<TestItem> TableItems { set; get; } = [];

    [ObservableProperty]
    private GridLength[] columnWidths = new[]
    {
        new GridLength(1, GridUnitType.Star),
        new GridLength(1, GridUnitType.Pixel)
    };


    public DayViewModel() : base() {
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


    public async Task GetTasksAsync() {

	}

	public string GetDateString() {
		return "Montag 14.3.";
	}

	public record TestItem();
}
