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


	public EventDispatcher() {
		callback = args => { };
	}

	public EventDispatcher(Action<T> handler) : this() {
		callback += handler as Action<EventArgs>;
	}


	public void Subscribe(Action<T> handler) {
		callback += handler;
	}

	public void UnSubscribe(Action<T> handler) {
		callback -= handler;
	}

	public void Invoke(T args) {
		callback?.Invoke(args);
	}

	public void Clear() {
		callback = (args) => { };
	}
}