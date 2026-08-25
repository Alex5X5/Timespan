namespace Timespan.GUI.Types;

public class ObservableBool : ReactiveObject {

	private bool _value;
	public bool Value {
		get => _value;
		set => this.RaiseAndSetIfChanged(ref _value, value);
	}

	public ObservableBool(bool? initial = false) {
		_value = initial ?? false;
	}

	public override string ToString() =>
		$"Observable[{Value}]";
}