namespace Timespan.GUI.Services; 

public class GlobalEventService {
	
	private static readonly Dictionary<Type, EventDispatcherBase> _store = [];

	public static EventDispatcher<T> GetEvent<T>() where T : EventArgs {
		var key = typeof(T);
		if(!_store.ContainsKey(key))
			_store[key] = new EventDispatcher<T>();
		return (EventDispatcher<T>)_store[key];
	}

	public static void Raise<T>(T args) where T : EventArgs {
		GetEvent<T>().Invoke(args);
	}

	public static void Raise<T>() where T : EventArgs, new(){
		GetEvent<T>().Invoke(new T());
	}
}

public class EventDispatcherBase {
}

public partial class EventDispatcher<T> : EventDispatcherBase where T : EventArgs {

	private Action<T>? callback;

	public static EventDispatcher<T> operator +(EventDispatcher<T> dispatcher, Action<T> handler) {
		dispatcher.callback += handler;
		return dispatcher;
	}

	public static EventDispatcher<T> operator -(EventDispatcher<T> dispatcher, Action<T> handler) {
		dispatcher.callback = (dispatcher.callback - handler) ?? (args => {});
		return dispatcher;
	}

	public EventDispatcher() {
	}

	public EventDispatcher(Action<T> callback) {
		this.callback += callback as Action<EventArgs>;
	}

	public void Invoke(T args) {
		callback?.Invoke(args);
	}

	public void Clear() {
		callback = (args) => { };
	}
}