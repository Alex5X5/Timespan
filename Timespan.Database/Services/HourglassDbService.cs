namespace Timespan.Database.Services;

using Avalonia.Media;
using DatabaseUtil;

using Timespan.Database.Models;
using Timespan.Database.Services.Interfaces;
using Timespan.Util.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class HourglassDbService : IHourglassDbService {

	DatabaseAccessor<HourglassDbContext> _accessor = 
		new(PathService.FilesPath("database"), DatabasePathFormat.FileName, null);


    public async Task<bool> UpdateTaskAsync(Models.Task updatedTask) =>
        await _accessor.UpdateAsync(updatedTask, false);

    public async System.Threading.Tasks.Task DeleteTaskAsync(Models.Task task) =>
        await _accessor.DeleteAsync(task);


    public async Task<Models.Task> StartNewTaskAsnc(string description, Color color, Project? project, Worker worker, Ticket? ticket) {
        long now = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
        Models.Task task = new() {
            DisplayColor = color,
            description = description,
            blocksTime = BlockedTimeIntervallType.None,
            owner = worker,
            project = project,
            running = true,
            start = now,
            finish = now
        };
        await _accessor.AddAsync(task, false);
        return task;
    }

    public async Task<Models.Task> ContiniueTaskAsync(Models.Task taskToContiniue) { 
		Models.Task? runningTask = await QueryCurrentTaskAsync();
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

	public async Task<Models.Task?> FinishCurrentTaskAsync(long? start, long? finish, string description, Project? project, Ticket? ticket) {
		Models.Task? current = await QueryCurrentTaskAsync();
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


    public async Task<Models.Task> CreateIntervallBlockingTaskAsync(BlockedTimeIntervallType type, DateTime date, long duration) {
        string reason = type switch {
            BlockedTimeIntervallType.Vacant => "Urlaub",
            BlockedTimeIntervallType.Holiday => "Feiertag",
            BlockedTimeIntervallType.Sick => "Krank",
            BlockedTimeIntervallType.NoExcuse => "Unentschuldigt",
            BlockedTimeIntervallType.None => "",
            _ => ""
        };
        Models.Task task = new() {
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
