namespace Timespan.GUI.Services.Mapping;

using Timespan.GUI.Types;

public class TaskMapper {

	public static ObservableTask ToGuiType(Timespan.Types.Models.Task task) {
		return new(task);
	}

	public static Timespan.Types.Models.Task ToSharedType(ObservableTask task) {
		return task.Value;
	}
}
