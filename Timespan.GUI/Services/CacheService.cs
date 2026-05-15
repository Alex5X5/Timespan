namespace Timespan.GUI.Services;

using System;

using Util = Hourglass.Util;
using Hourglass.Database.Services.Interfaces;
using Models = Hourglass.Database.Models;


public class CacheService : Util.Services.CacheService {

    private Models.Task? runningTask;
    public Models.Task? RunningTask {
        set {
            runningTask = value?.Clone();
            OnRunningTaksChanged?.Invoke(runningTask);
        }
        get => runningTask;
    }
    public event Action<Models.Task?>? OnRunningTaksChanged;

    private Models.Task? selectedTask;
    public Models.Task? SelectedTask {
        set {
            selectedTask = value?.Clone();
            OnSelectedTaksChanged?.Invoke(selectedTask);
        }
        get => selectedTask;
    }
    public event Action<Models.Task?>? OnSelectedTaksChanged;

    public CacheService(IHourglassDbService dbService) : base() {
        RunningTask = dbService.QueryCurrentTaskAsync().Result;
    }
}