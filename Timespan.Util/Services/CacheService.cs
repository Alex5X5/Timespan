namespace Timespan.Util.Services;

using System;

public class CacheService {

    private DateTime selectedDay = DateTimeService.FloorDay(DateTime.Today);

	public DateTime SelectedDay {
        set => selectedDay = value;
        get => selectedDay;
    }

    public CacheService() {
        
    }
}
