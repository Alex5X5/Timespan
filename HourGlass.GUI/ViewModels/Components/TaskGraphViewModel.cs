namespace Hourglass.GUI.ViewModels.Components;

using Hourglass.GUI.Views.Components.GraphPanels;
using ReactiveUI;

public class TaskGraphViewModel : ViewModelBase {

	private Database.Types.Task task;
	public Database.Types.Task Task {
        set => this.RaiseAndSetIfChanged(ref task, value);
        get => task;
    }

    public string DeescriptionStub => Task.description.Length <= GraphPanelViewBase.MAX_TASK_DESCRIPTION_CHARS ? Task.description : Task.description[..GraphPanelViewBase.MAX_TASK_DESCRIPTION_CHARS] + "...";

	private bool isRemovingLast = false;
	public bool IsRemovingLast {
		set {
			this.RaiseAndSetIfChanged(ref isRemovingLast, value);
			this.RaisePropertyChanged(nameof(IsRemoving));
			this.RaisePropertyChanged(nameof(Left));
		}
		get => isRemovingLast;
	}

	private bool isRemovingFollowing = false;
	public bool IsRemovingFollowing {
		set {
			this.RaiseAndSetIfChanged(ref isRemovingFollowing, value);
			this.RaisePropertyChanged(nameof(IsRemoving));
			this.RaisePropertyChanged(nameof(Right));
		}
		get => isRemovingFollowing;
	}

	private bool isAddingLast = false;
	public bool IsAddingLast {
		set {
			this.RaiseAndSetIfChanged(ref isAddingLast, value);
			this.RaisePropertyChanged(nameof(IsAdding));
			this.RaisePropertyChanged(nameof(Right));
		}
		get => isAddingLast;
	}

	private bool isAddingFollowing = false;
	public bool IsAddingFollowing {
		set {
			this.RaiseAndSetIfChanged(ref isAddingFollowing, value);
			this.RaisePropertyChanged(nameof(IsAdding));
			this.RaisePropertyChanged(nameof(Left));
		}
		get => isAddingFollowing;
	}

	public bool IsAdding {
		get => (isAddingFollowing | isAddingLast) && !IsRemoving;
	}

	public bool IsRemoving {
		get => isRemovingFollowing | isRemovingLast;
	}

	public bool Right {
		get => isRemovingFollowing | isAddingLast;
	}

	public bool Left {
		get => isRemovingLast | isAddingFollowing;
	}

	private int index = 0;
    public int Index {
        set => this.RaiseAndSetIfChanged(ref index, value);
        get => index;
    }

    public TaskGraphViewModel() : this(new Database.Types.Task()) { }

	public TaskGraphViewModel(Database.Types.Task task) {
		this.task = task;
    }
}
