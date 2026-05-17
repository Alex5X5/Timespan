namespace Timespan.Database.Services;

using Timespan.Database.Services.Interfaces;
using Timespan.Util.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class HourglassDbService {

    public async Task<List<Models.Task>> QueryTasksAsync() =>
        await _accessor.QueryAllAsync<Models.Task>();
    
    public async Task<List<Models.Task>> QueryTasksInIntervallAsync(long intervallStartSeconds, long intervallFinishSeconds) {
        List<Models.Task> tasks = (await _accessor.QueryAllAsync<Models.Task>());
        return tasks
            .Where(x => x.start >= intervallStartSeconds && x.start <= intervallFinishSeconds)
                .Where(x => x.finish >= intervallStartSeconds && x.finish <= intervallFinishSeconds)
                    .OrderBy(p => p.start)
                        .ToList();
    }

    public async Task<Models.Task?> QueryCurrentTaskAsync() {
        List<Models.Task> tasks = await QueryTasksAsync();
        Models.Task? task = (await QueryTasksAsync())
            .Where(t => t.running)
                .MaxBy(x => x.start);
        return task;
    }

    public async Task<List<Models.Task>> QueryTasksOfHourAtDateAsync(DateTime date) {
        return await QueryTasksInIntervallAsync(
            DateTimeService.ToSeconds(DateTimeService.FloorHour(date)),
            DateTimeService.ToSeconds(DateTimeService.FloorHour(date).AddHours(1))
        );
    }

    public async Task<List<Models.Task>> QueryTasksOfCurrentHourAsync() =>
        await QueryTasksOfHourAtDateAsync(DateTime.Now);

    public async Task<List<Models.Task>> QueryTasksOfDayAtDateAsync(DateTime date) {
        DateTime start = DateTimeService.FloorDay(date);
        DateTime finfish = start.AddDays(1);
        return await QueryTasksInIntervallAsync(DateTimeService.ToSeconds(start), DateTimeService.ToSeconds(finfish));
    }

    public async Task<List<Models.Task>> QueryTasksOfCurrentDayAsync() =>
        await QueryTasksOfDayAtDateAsync(DateTime.Now);

    public async Task<List<Models.Task>> QueryTasksOfWeekAtDateAsync(DateTime date) {
        DateTime start = DateTimeService.FloorWeek(date);
        DateTime finfish = start.AddDays(7);
        return await QueryTasksInIntervallAsync(DateTimeService.ToSeconds(start), DateTimeService.ToSeconds(finfish));
    }

    public async Task<List<Models.Task>> QueryTasksOfCurrentWeekAsync() =>
        await QueryTasksOfWeekAtDateAsync(DateTime.Now);

    public async Task<List<Models.Task>> QueryTasksOfMonthAtDateAsync(DateTime date) {
        DateTime start = DateTimeService.FloorMonth(date);
        DateTime finfish = start.AddDays(DateTime.DaysInMonth(date.Year, date.Month));
        return await QueryTasksInIntervallAsync(DateTimeService.ToSeconds(start), DateTimeService.ToSeconds(finfish));
    }

    public async Task<List<Models.Task>> QueryTasksOfCurrentMonthAsync() =>
        await QueryTasksOfMonthAtDateAsync(DateTime.Now);

    
    private async Task<IEnumerable<Models.Task>> QueryAllIntervallBlockingTasksAsync() =>
        (await QueryTasksAsync())
            .Where(x => x.blocksTime != BlockedTimeIntervallType.None);

    public async Task<List<Models.Task>> QueryBlockingTasksInIntervallAsync(long intervallStartSeconds, long intervallFinishSeconds) {
        List<Models.Task> tasks = (await _accessor.QueryAllAsync<Models.Task>());
        return tasks.Where(x => x.blocksTime != BlockedTimeIntervallType.None)
            .Where(x => x.start >= intervallStartSeconds && x.start <= intervallFinishSeconds)
                .Where(x => x.finish >= intervallStartSeconds && x.finish <= intervallFinishSeconds)
                    .OrderBy(p => p.start)
                        .ToList();
    }

    public async Task<List<Models.Task>> QueryBlockingTasksAtDateAsync(DateTime date) {
        DateTime hour = DateTimeService.FloorHour(date);
        DateTime day = DateTimeService.FloorDay(date);
        DateTime week = DateTimeService.FloorWeek(date);
        IEnumerable<Models.Task> tasks = await QueryBlockingTasksInIntervallAsync(DateTimeService.ToSeconds(week), DateTimeService.ToSeconds(week.AddDays(7)));
        return tasks.Where(x => x.StartDateTime == hour && x.Duration == TimeSpan.SecondsPerHour)
            .Concat(tasks.Where(x => x.StartDateTime == day && x.Duration == TimeSpan.SecondsPerDay))
                .Concat(tasks.Where(x => x.StartDateTime == week && x.Duration == TimeSpan.SecondsPerDay * 7))
                    .ToList();
    }
}
