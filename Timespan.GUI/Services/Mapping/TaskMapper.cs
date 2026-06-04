namespace Timespan.GUI.Services.Mapping;

using Timespan.GUI.Types;

public class TaskMapper {

	public static ObservableTask ToDomain(Timespan.Types.Models.Task task) {
		return new(task);
	}

	public static Timespan.Types.Models.Task ToShared(ObservableTask task) {
		return task.Value;
	}
}
