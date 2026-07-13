namespace Timespan.GUI.Services; 

public class GlobalEventService {
	
	private static readonly Dictionary<Type, Dictionary<string, EventDispatcherBase>> _store = [];

	public static EventDispatcher<T> GetEvent<T>(string descriptor = "e") where T : EventArgs {
		var key = typeof(T);
		if(!_store.ContainsKey(key))
			_store[key] = new Dictionary<string, EventDispatcherBase>();
		if(!_store[key].ContainsKey(descriptor))
			_store[key][descriptor] = new EventDispatcher<T>();
		return (EventDispatcher<T>)_store[key][descriptor];
	}

	public static void Subscribe<T>(Action<T> handler, string descriptor = "e") where T : EventArgs {
		GetEvent<T>(descriptor).Subscribe(handler);
	}

	public static void UnSubscribe<T>(Action<T> handler, string descriptor = "e") where T : EventArgs {
		GetEvent<T>(descriptor).UnSubscribe(handler);
	}

	public static void Raise<T>(T args, string descriptor = "e") where T : EventArgs {
		GetEvent<T>(descriptor).Invoke(args);
	}

	public static void Raise<T>(string descriptor = "e") where T : EventArgs, new(){
		GetEvent<T>(descriptor).Invoke(new T());
	}
}

public class EventDispatcherBase {
}

public class EventDispatcher<T> : EventDispatcherBase where T : EventArgs {

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