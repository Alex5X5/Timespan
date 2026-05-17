namespace Hourglass.GUI.Services;

using Timespan.Database.Services.Interfaces;

using System;

public class CacheService : Util.Services.CacheService {

    private Database.Models.Task? runningTask;
    public Database.Models.Task? RunningTask {
        set {
            runningTask = value?.Clone();
            OnRunningTaksChanged?.Invoke(runningTask);
        }
        get => runningTask;
    }
    public event Action<Database.Models.Task?>? OnRunningTaksChanged;

    private Database.Models.Task? selectedTask;
    public Database.Models.Task? SelectedTask {
        set {
            selectedTask = value?.Clone();
            OnSelectedTaksChanged?.Invoke(selectedTask);
        }
        get => selectedTask;
    }
    public event Action<Database.Models.Task?>? OnSelectedTaksChanged;

    public CacheService(IHourglassDbService dbService) : base() {
        RunningTask = dbService.QueryCurrentTaskAsync().Result;
    }
}