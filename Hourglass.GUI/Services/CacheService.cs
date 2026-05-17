namespace Hourglass.GUI.Services;

using Timespan.Database.Services.Interfaces;

using System;

public class CacheService : Util.Services.CacheService {

    private Database.Types.Task? runningTask;
    public Database.Types.Task? RunningTask {
        set {
            runningTask = value?.Clone();
            OnRunningTaksChanged?.Invoke(runningTask);
        }
        get => runningTask;
    }
    public event Action<Database.Types.Task?>? OnRunningTaksChanged;

    private Database.Types.Task? selectedTask;
    public Database.Types.Task? SelectedTask {
        set {
            selectedTask = value?.Clone();
            OnSelectedTaksChanged?.Invoke(selectedTask);
        }
        get => selectedTask;
    }
    public event Action<Database.Types.Task?>? OnSelectedTaksChanged;

    public CacheService(IHourglassDbService dbService) : base() {
        RunningTask = dbService.QueryCurrentTaskAsync().Result;
    }
}