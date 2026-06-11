namespace Timespan.GUI.Services;

using System;

using Timespan.Database.Services.Interfaces;

using Types = Timespan.Types.Models;
using Util = Timespan.Util;


public class CacheService : Util.Services.CacheService {

    private Types.Task? runningTask;
    public Types.Task? RunningTask {
        set {
            runningTask = value?.Clone();
            OnRunningTaksChanged?.Invoke(runningTask);
        }
        get => runningTask;
    }
    public event Action<Types.Task?>? OnRunningTaksChanged;

    private Types.Task? selectedTask;
    public Types.Task? SelectedTask {
        set {
            selectedTask = value?.Clone();
            OnSelectedTaksChanged?.Invoke(selectedTask);
        }
        get => selectedTask;
    }
    public event Action<Types.Task?>? OnSelectedTaksChanged;

    public CacheService() : base() {
    }
}