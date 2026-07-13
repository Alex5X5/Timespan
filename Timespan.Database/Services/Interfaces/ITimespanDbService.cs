namespace Timespan.Database.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Media;

using Types = Timespan.Types.Models;

public interface ITimespanDbService {

	public Task<List<Types.Task>> QueryTasksAsync();

	public Task<Types.Task?> QueryTasksByIdAsync(int id);

	public Task<Types.Task?> QueryCurrentTaskAsync();

    public Task<List<Types.Task>> QueryTasksOfHourAtDateAsync(DateTime date);

    public Task<List<Types.Task>> QueryTasksOfCurrentHourAsync();

    public Task<List<Types.Task>> QueryTasksOfDayAtDateAsync(DateTime date);

    public Task<List<Types.Task>> QueryTasksOfCurrentDayAsync();

    public Task<List<Types.Task>> QueryTasksOfWeekAtDateAsync(DateTime date);

    public Task<List<Types.Task>> QueryTasksOfCurrentWeekAsync();

    public Task<List<Types.Task>> QueryTasksOfMonthAtDateAsync(DateTime date);

    public Task<List<Types.Task>> QueryTasksOfCurrentMonthAsync();


    public Task<bool> UpdateTaskAsync(Types.Task updatedTask);

	public Task DeleteTaskAsync(Types.Task updatedTask);

	
    public Task<Types.Task> StartNewTaskAsnc(string description, Color color, Types.Project? project, Types.Worker worker, Types.Ticket? ticket);
	
    public Task<Types.Task?> FinishCurrentTaskAsync(long? start, long? finish, string description, Types.Project? project, Types.Ticket? ticket);

	public Task<Types.Task> ContiniueTaskAsync(Types.Task updatedTask);


    public Task<Types.Task> CreateIntervallBlockingTaskAsync(Types.BlockedTimeIntervallType type, DateTime date, long duration);

	public Task<List<Types.Task>> QueryBlockingTasksAtDateAsync(DateTime date);

	public Task<List<Types.Task>> QueryBlockingTasksInIntervallAsync(long startSeconds, long finishSeconds);

    public Task<string?> GetHourBlockedMessageAsync(DateTime date);

    public Task<string?> GetDayBlockedMessageAsync(DateTime date);

    public Task<string?> GetWeekBlockedMessageAsync(DateTime date);
}
