namespace Timespan.Util.Services;

using System;

public class CacheService {

    private long SelectedDayStartSeconds = DateTimeService.ToSeconds(DateTimeService.FloorDay(DateTime.Today));
    public DateTime SelectedDay {
        set {
            Console.WriteLine($"settin selected day to {value}");
            SelectedDayStartSeconds = DateTimeService.ToSeconds(value);
            OnSelectedDayChanged?.Invoke(SelectedDay);
        }
        get => new(SelectedDayStartSeconds * TimeSpan.TicksPerSecond);
    }
    public event Action<DateTime?>? OnSelectedDayChanged = date => { };

    public CacheService() {
        
    }
}
