namespace Timespan.GUI.Services; 

using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

public class GlobalEventService {


	private interface IEntry { }


	private sealed class Entry<TValue>(TValue value) : IEntry {
		public TValue Value { get; set; } = value;
	}


	private readonly Dictionary<Type, Dictionary<string, IEntry>> _store;

	public GlobalEventService() {
		_store = [];
	}

	internal void Register<OwnerT, ChildT>(RedirectionAnchor<ChildT> newAnchor, string key = "anchor") {
		Set<OwnerT, RedirectionAnchor<ChildT>>(key, newAnchor);
	}
	
	internal RedirectionAnchor<ChildT>? GetAnchor<OwnerT, ChildT>(string key = "anchor") {
		return Get<OwnerT, RedirectionAnchor<ChildT>>(key);
	}

	private void Set<TOwner, TValue>(string key, TValue value) {
		if (!_store.TryGetValue(typeof(TOwner), out var inner))
			_store[typeof(TOwner)] = inner = new();
		inner[key] = new Entry<TValue>(value);
	}

	private TValue? Get<TOwner, TValue>(string key) {
		if (_store.TryGetValue(typeof(TOwner), out var inner)
			&& inner.TryGetValue(key, out var entry)
			&& entry is Entry<TValue> typed)
			return typed.Value;
		return default;
	}
}


public partial class EventRouter : ObservableObject {

	public EventRouter() {
		
	}
}