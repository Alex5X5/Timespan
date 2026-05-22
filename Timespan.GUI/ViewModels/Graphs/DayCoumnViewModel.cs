using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace Timespan.GUI.ViewModels.Graphs;

public partial class DayCoumnViewModel : ViewModelBase, IGraphsViewChild {

	[ObservableProperty]
	private bool[] showColumn = new bool[24];

	public DayCoumnViewModel() : base() {
		for(int i=0; i<showColumn.Length; i++) {
			if (i < 6) { 
				showColumn[i] = false;
			} else if(i > 17) {
				showColumn[i] = false;
			} else {
				showColumn[i] = true;
			}
		};
	}

	public async Task GetTasksAsync() {

	}

	public string GetDateString() {
		return "Montag 14.3.";
	}
}
