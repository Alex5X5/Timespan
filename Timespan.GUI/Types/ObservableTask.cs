using Avalonia.Media;

using CommunityToolkit.Mvvm.ComponentModel;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Timespan.Types.Models;

namespace Timespan.GUI.Types;

public class ObservableTask: ReactiveObject, IReactiveObject {

	private Timespan.Types.Models.Task _value;
	public Timespan.Types.Models.Task Value {
		set => this.RaiseAndSetIfChanged(ref _value, value);
		get => _value;
	}

	public long Id {
		set {
			_value.Id = value;
			this.RaisePropertyChanged(nameof(Id));
			this.RaisePropertyChanged(nameof(Value));
		}
		get => _value.Id;
	}

	public string Description {
		set {
			_value.description = value;
			this.RaisePropertyChanged(nameof(Description));
			this.RaisePropertyChanged(nameof(Value));
		}
		get => _value.description;
	}

	public bool Running {
		set {
			_value.running = value;
			this.RaisePropertyChanged(nameof(Running));
			this.RaisePropertyChanged(nameof(Value));
		}
		get => _value.running;
	}

	public long Start {
		set {
			_value.start = value;
			this.RaisePropertyChanged(nameof(Start));
			this.RaisePropertyChanged(nameof(Value));
		}
		get => _value.start;
	}

	public long Finish {
		set {
			_value.finish = value;
			this.RaisePropertyChanged(nameof(Finish));
			this.RaisePropertyChanged(nameof(Value));
		}
		get => _value.finish;
	}

	public byte DisplayColorRed {
		set {
			_value.displayColorRed = value;
			this.RaisePropertyChanged(nameof(DisplayColorRed));
			this.RaisePropertyChanged(nameof(Value));
		}
		get => _value.displayColorRed;
	}

	public byte DisplayColorGreen {
		set {
			_value.displayColorGreen = value;
			this.RaisePropertyChanged(nameof(DisplayColorGreen));
			this.RaisePropertyChanged(nameof(Value));
		}
		get => _value.displayColorGreen;
	}

	public byte DisplayColorBlue {
		set {
			_value.displayColorBlue = value;
			this.RaisePropertyChanged(nameof(DisplayColorBlue));
			this.RaisePropertyChanged(nameof(Value));
		}
		get => _value.displayColorBlue;
	}

	public BlockedTimeIntervallType BlocksTime {
		set {
			_value.blocksTime = value;
			this.RaisePropertyChanged(nameof(BlocksTime));
			this.RaisePropertyChanged(nameof(Value));
		}
		get => _value.blocksTime;
	}

	public long Duration => _value.Duration;

	public DateTime StartDateTime => _value.StartDateTime;

	public DateTime FinishDateTime => _value.FinishDateTime;

	public Color DisplayColor {
		set {
			DisplayColorRed = value.R;
			DisplayColorGreen = value.G;
			DisplayColorBlue = value.B;
		}
		get => new(255, DisplayColorRed, DisplayColorGreen, DisplayColorBlue);
	}

	private int cellIndex = 0;

	public int CellIndex {
		set {
			cellIndex = value;
			this.RaisePropertyChanged(nameof(CellIndex));
		}
		get => cellIndex;
	}
	
	public ObservableTask(Timespan.Types.Models.Task? initial) {
		_value = initial ?? new();
	}

	public override string ToString() =>
		$"Observable[{Value.ToString}]";
}