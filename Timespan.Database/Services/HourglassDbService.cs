namespace Timespan.Database.Services;

using Avalonia.Media;
using DatabaseUtil;

using Types = Timespan.Types.Models;
using Timespan.Database.Services.Interfaces;
using Timespan.Util.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class HourglassDbService : IHourglassDbService {

	DatabaseAccessor<HourglassDbContext> _accessor = 
		new(PathService.FilesPath("database"), DatabasePathFormat.FileName, null);


    public async Task<bool> UpdateTaskAsync(Types.Task updatedTask) =>
        await _accessor.UpdateAsync(updatedTask, false);

    public async Task DeleteTaskAsync(Types.Task task) =>
        await _accessor.DeleteAsync(task);


    public async Task<Types.Task> StartNewTaskAsnc(string description, Color color, Types.Project? project, Types.Worker worker, Types.Ticket? ticket) {
        long now = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
        Types.Task task = new() {
            DisplayColor = color,
            description = description,
            blocksTime = Types.BlockedTimeIntervallType.None,
            owner = worker,
            project = project,
            running = true,
            start = now,
            finish = now
        };
        await _accessor.AddAsync(task, false);
        return task;
    }

    public async Task<Types.Task> ContiniueTaskAsync(Types.Task taskToContiniue) { 
		Types.Task? runningTask = await QueryCurrentTaskAsync();
		if (runningTask != null)
			await FinishCurrentTaskAsync(
				runningTask.start,
				runningTask.finish,
				runningTask.description,
				runningTask.project,
				runningTask.ticket
			);
		taskToContiniue.finish = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
		taskToContiniue.running = true;
		await _accessor.UpdateAsync(taskToContiniue, false);
		return taskToContiniue;
	}

	public async Task<Types.Task?> FinishCurrentTaskAsync(long? start, long? finish, string description, Types.Project? project, Types.Ticket? ticket) {
		Types.Task? current = await QueryCurrentTaskAsync();
		if (current == null)
			return null;
		if (start != null)
			current.start = (long)start;
		current.description = description;
		current.finish = finish ?? DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
		current.running = false;
		if(await _accessor.UpdateAsync(current, false))
			return null;
		return current;
	}


    public async Task<Types.Task> CreateIntervallBlockingTaskAsync(Types.BlockedTimeIntervallType type, DateTime date, long duration) {
        string reason = type switch {
            Types.BlockedTimeIntervallType.Vacant => "Urlaub",
			Types.BlockedTimeIntervallType.Holiday => "Feiertag",
			Types.BlockedTimeIntervallType.Sick => "Krank",
            Types.BlockedTimeIntervallType.NoExcuse => "Unentschuldigt",
            Types.BlockedTimeIntervallType.None => "",
            _ => ""
        };
        Types.Task task = new() {
            description = reason,
            blocksTime = type,
            owner = null,
            project = null,
            running = false,
            StartDateTime = date,
            FinishDateTime = date.AddSeconds(duration)
        };
        await _accessor.AddAsync(task, false);
        return task;
    }

    public async Task<string?> GetHourBlockedMessageAsync(DateTime date) {
        long seconds = DateTimeService.ToSeconds(DateTimeService.FloorHour(date));
        return (await QueryBlockingTasksInIntervallAsync(seconds, TimeSpan.SecondsPerHour))
            .FirstOrDefault(x => x.start == seconds)?.description;
    }

    public async Task<string?> GetDayBlockedMessageAsync(DateTime date) {
        long seconds = DateTimeService.ToSeconds(DateTimeService.FloorDay(date));
        return (await QueryAllIntervallBlockingTasksAsync())
            .FirstOrDefault(x => x.start == seconds)?.description;
    }

    public async Task<string?> GetWeekBlockedMessageAsync(DateTime date) {
        long seconds = DateTimeService.ToSeconds(DateTimeService.FloorWeek(date));
        return (await QueryAllIntervallBlockingTasksAsync())
            .FirstOrDefault(x => x.start == seconds)?.description;
    }

}
