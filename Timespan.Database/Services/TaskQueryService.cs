namespace Timespan.Database.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Types = Timespan.Types.Models;
using Timespan.Util.Services;

public partial class TimespanDbService {

	public async Task<List<Types.Task>> QueryTasksAsync() =>
		await _accessor.QueryAllAsync<Types.Task>();

	public async Task<Types.Task?> QueryTasksByIdAsync(long id) =>
		await _accessor.QuerySingleByKeyAsync<Types.Task>(id);
	
	public async Task<List<Types.Task>> QueryTasksInIntervallAsync(long intervallStartSeconds, long intervallFinishSeconds) {
		List<Types.Task> tasks = await QueryTasksAsync();
		return tasks
			.Where(x => x.start >= intervallStartSeconds && x.start <= intervallFinishSeconds)
				.Where(x => x.finish >= intervallStartSeconds && x.finish <= intervallFinishSeconds)
					.Where(x => x.blocksTime == Types.BlockedTimeIntervallType.None)
						.OrderBy(p => p.start)
							.ToList();
	}

	public async Task<Types.Task?> QueryCurrentTaskAsync() {
		List<Types.Task> tasks = await QueryTasksAsync();
		Types.Task? task = (await QueryTasksAsync())
			.Where(t => t.running)
				.MaxBy(x => x.start);
		return task;
	}

	public async Task<List<Types.Task>> QueryTasksOfHourAtDateAsync(DateTime date) {
		return await QueryTasksInIntervallAsync(
			DateTimeService.ToSeconds(DateTimeService.FloorHour(date)),
			DateTimeService.ToSeconds(DateTimeService.FloorHour(date).AddHours(1))
		);
	}

	public async Task<List<Types.Task>> QueryTasksOfCurrentHourAsync() =>
		await QueryTasksOfHourAtDateAsync(DateTime.Now);

	public async Task<List<Types.Task>> QueryTasksOfDayAtDateAsync(DateTime date) {
		DateTime start = DateTimeService.FloorDay(date);
		DateTime finfish = start.AddDays(1);
		return await QueryTasksInIntervallAsync(DateTimeService.ToSeconds(start), DateTimeService.ToSeconds(finfish));
	}

	public async Task<List<Types.Task>> QueryTasksOfCurrentDayAsync() =>
		await QueryTasksOfDayAtDateAsync(DateTime.Now);

	public async Task<List<Types.Task>> QueryTasksOfWeekAtDateAsync(DateTime date) {
		DateTime start = DateTimeService.FloorWeek(date);
		DateTime finfish = start.AddDays(5);
		return await QueryTasksInIntervallAsync(DateTimeService.ToSeconds(start), DateTimeService.ToSeconds(finfish));
	}

	public async Task<List<Types.Task>> QueryTasksOfCurrentWeekAsync() =>
		await QueryTasksOfWeekAtDateAsync(DateTime.Now);

	public async Task<List<Types.Task>> QueryTasksOfMonthAtDateAsync(DateTime date) {
		DateTime start = DateTimeService.FloorMonth(date);
		DateTime finfish = start.AddDays(DateTime.DaysInMonth(date.Year, date.Month));
		return await QueryTasksInIntervallAsync(DateTimeService.ToSeconds(start), DateTimeService.ToSeconds(finfish));
	}

	public async Task<List<Types.Task>> QueryTasksOfCurrentMonthAsync() =>
		await QueryTasksOfMonthAtDateAsync(DateTime.Now);

	
	private async Task<IEnumerable<Types.Task>> QueryAllIntervallBlockingTasksAsync() =>
		(await QueryTasksAsync())
			.Where(x => x.blocksTime != Types.BlockedTimeIntervallType.None);

	public async Task<List<Types.Task>> QueryBlockingTasksInIntervallAsync(long intervallStartSeconds, long intervallFinishSeconds) {
		IEnumerable<Types.Task> tasks = (await _accessor.QueryAllAsync<Types.Task>());
		tasks = tasks.Where(x => x.blocksTime != Types.BlockedTimeIntervallType.None);
		tasks = tasks.Where(x => x.start >= intervallStartSeconds && x.start <= intervallFinishSeconds);
		tasks = tasks.Where(x => x.finish >= intervallStartSeconds && x.finish <= intervallFinishSeconds);
		return tasks
			.OrderBy(p => p.start)
			.ToList();
	}

	public async Task<List<Types.Task>> QueryBlockingTasksAtDateAsync(DateTime date) {
		DateTime hour = DateTimeService.FloorHour(date);
		DateTime day = DateTimeService.FloorDay(date);
		DateTime week = DateTimeService.FloorWeek(date);
		IEnumerable<Types.Task> tasks = await QueryBlockingTasksInIntervallAsync(DateTimeService.ToSeconds(week), DateTimeService.ToSeconds(week.AddDays(7)));
		return tasks.Where(x => x.StartDateTime == hour && x.Duration == TimeSpan.SecondsPerHour)
			.Concat(tasks.Where(x => x.StartDateTime == day && x.Duration == TimeSpan.SecondsPerDay))
				.Concat(tasks.Where(x => x.StartDateTime == week && x.Duration == TimeSpan.SecondsPerDay * 7))
					.ToList();
	}
}
