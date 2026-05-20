namespace Timespan.GUI.Services;

internal class Registry {

	private interface IEntry { }
	
	private sealed class Entry<TValue>(TValue value) : IEntry {
		public TValue Value { get; set; } = value;
	}

	private readonly Dictionary<Type, Dictionary<string, IEntry>> _store;
	
	public Registry() {
		_store = [];
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
