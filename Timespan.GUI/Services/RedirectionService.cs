namespace Timespan.GUI.Services; 

using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.DependencyInjection;

public class RedirectionService {

	private interface IEntry { }

	private sealed class Entry<TValue>(TValue value) : IEntry {
		public TValue Value { get; set; } = value;
	}


	private readonly Dictionary<Type, Dictionary<string, IEntry>> _store;

	public RedirectionService() {
		_store = [];
	}

	internal void Register<OwnerT, ChildT>(IRedirectionAnchor<ChildT> newAnchor, string key = "anchor") {
		Set<OwnerT, IRedirectionAnchor<ChildT>>(key, newAnchor);
	}

	internal void Register<OwnerT, ChildT>(RedirectionAnchor<ChildT> newAnchor, string key = "anchor") {
		Set<OwnerT, IRedirectionAnchor<ChildT>>(key, newAnchor);
	}

	internal void Register<OwnerT, ChildT>(ScopedRedirectionAnchor<ChildT> newAnchor, string key = "anchor") {
		Set<OwnerT, IRedirectionAnchor<ChildT>>(key, newAnchor);
	}

	internal IRedirectionAnchor<ChildT>? GetAnchor<OwnerT, ChildT>(string key = "anchor") {
		return Get<OwnerT, IRedirectionAnchor<ChildT>>(key);
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

public interface IRedirectionAnchor<ChildT> {
	public ChildT? CurrentModel { set; get; }

	public event Action<Type?, Type> ModelChanged;

	public bool IsActive<T>();

	public void ChangeModel<T>(Action<T?>? afterChange = null) where T : ChildT;

	public void GoBack();
}

public partial class RedirectionAnchor<ChildT> : ObservableObject, IRedirectionAnchor<ChildT> {

	[ObservableProperty]
	private ChildT? currentModel;

	private ChildT? lastModel;

	public event Action<Type?, Type> ModelChanged = (from, to) => { };

	public RedirectionAnchor() { }

	public bool IsActive<T>() =>
		typeof(T) == CurrentModel?.GetType();

	public void ChangeModel<T>(Action<T?>? afterChange=null) where T : ChildT {
		lastModel = CurrentModel;
		CurrentModel = App.Current.Services.GetService<T>();
		ModelChanged.Invoke(CurrentModel?.GetType(), typeof(T));
		afterChange?.Invoke((T?)CurrentModel);
	}

	public void GoBack() {
		if (lastModel != null) {
			var _CurrentModel = CurrentModel;
			CurrentModel = lastModel;
			ModelChanged.Invoke(_CurrentModel?.GetType(), lastModel.GetType());
		}
	}
}


public partial class ScopedRedirectionAnchor<ChildT> : ObservableObject, IRedirectionAnchor<ChildT> {

	[ObservableProperty]
	private ChildT? currentModel;

	private ChildT? lastModel;

	private IServiceScope? scope;

	public event Action<Type?, Type> ModelChanged = (from, to) => { };

	private readonly IServiceScopeFactory scopeFactory;

	public ScopedRedirectionAnchor(IServiceScopeFactory scopeFactory) {
		this.scopeFactory = scopeFactory;
	}

	public bool IsActive<T>() =>
		typeof(T) == CurrentModel?.GetType();

	public void CreateScope() {
		scope = scopeFactory.CreateScope();
	}

	public void CloseScope() {
		scope?.Dispose();
	}

	public void ChangeModel<T>(Action<T?>? afterChange = null) where T : ChildT {
		if (scope == null)
			throw new InvalidOperationException("can not change model while scope is null");
		lastModel = CurrentModel;
		CurrentModel = scope.ServiceProvider.GetService<T>();
		ModelChanged.Invoke(CurrentModel?.GetType(), typeof(T));
		afterChange?.Invoke((T?)CurrentModel);
	}

	public void GoBack() {
		if (scope == null)
			throw new InvalidOperationException("can not change model while scope is null");
		if (lastModel != null) {
			var _CurrentModel = CurrentModel;
			CurrentModel = lastModel;
			ModelChanged.Invoke(_CurrentModel?.GetType(), lastModel.GetType());
		}
	}
}