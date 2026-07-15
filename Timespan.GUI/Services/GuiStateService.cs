namespace Timespan.GUI.Services;

using CommunityToolkit.Mvvm.ComponentModel;

using Timespan.Util.Services;

public partial class GuiStateService : ObservableObject {
	
	private CacheService cacheService;

	[ObservableProperty]
	private Color selectedColor = new Color(0, 0, 0, 0);

	[ObservableProperty]
	private DateTime selectedDay = DateTime.MinValue;

	[ObservableProperty]
	private Timespan.Types.Models.Task selectedTask;

	[ObservableProperty]
	private Timespan.Types.Models.Task? runningTask;

	public GuiStateService(CacheService cacheService) : base() {
		this.cacheService = cacheService;
	}

	partial void OnSelectedDayChanged(DateTime value) {
		cacheService.SelectedDay = value;
	}
}
