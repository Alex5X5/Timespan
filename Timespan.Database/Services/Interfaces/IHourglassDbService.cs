namespace Timespan.Database.Services.Interfaces;

using Avalonia.Media;
using Timespan.Database.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IHourglassDbService {


	public Task<List<Models.Task>> QueryTasksAsync();

	public Task<Models.Task?> QueryCurrentTaskAsync();

    public Task<List<Models.Task>> QueryTasksOfHourAtDateAsync(DateTime date);

    public Task<List<Models.Task>> QueryTasksOfCurrentHourAsync();

    public Task<List<Models.Task>> QueryTasksOfDayAtDateAsync(DateTime date);

    public Task<List<Models.Task>> QueryTasksOfCurrentDayAsync();

    public Task<List<Models.Task>> QueryTasksOfWeekAtDateAsync(DateTime date);

    public Task<List<Models.Task>> QueryTasksOfCurrentWeekAsync();

    public Task<List<Models.Task>> QueryTasksOfMonthAtDateAsync(DateTime date);

    public Task<List<Models.Task>> QueryTasksOfCurrentMonthAsync();


    public Task<bool> UpdateTaskAsync(Models.Task updatedTask);

	public System.Threading.Tasks.Task DeleteTaskAsync(Models.Task updatedTask);

	
    public Task<Models.Task> StartNewTaskAsnc(string description, Color color, Project? project, Worker worker, Ticket? ticket);
	
    public Task<Models.Task?> FinishCurrentTaskAsync(long? start, long? finish, string description, Project? project, Ticket? ticket);

	public Task<Models.Task> ContiniueTaskAsync(Models.Task updatedTask);


    public Task<Models.Task> CreateIntervallBlockingTaskAsync(BlockedTimeIntervallType type, DateTime date, long duration);

	public Task<List<Models.Task>> QueryBlockingTasksAtDateAsync(DateTime date);

	public Task<List<Models.Task>> QueryBlockingTasksInIntervallAsync(long startSeconds, long finishSeconds);

    public Task<string?> GetHourBlockedMessageAsync(DateTime date);

    public Task<string?> GetDayBlockedMessageAsync(DateTime date);

    public Task<string?> GetWeekBlockedMessageAsync(DateTime date);
}
