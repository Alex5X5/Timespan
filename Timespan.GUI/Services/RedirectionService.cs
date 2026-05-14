namespace Timespan.GUI.Services; 

using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

internal class RedirectionService {

	private readonly Dictionary<Type, Dictionary<string, RedirectionAnchor<object>?>> registeredAnchors;

	public RedirectionService() {
		registeredAnchors = [];
	}

	internal void RegisterRedirectionAnchor<OwnerT, ChildT>(RedirectionAnchor<ChildT> newAnchor, string key = "anchor") {
		if (!registeredAnchors.TryGetValue(typeof(OwnerT), out var anchors)) {
			registeredAnchors.Add(typeof(OwnerT), new() { { key, newAnchor as RedirectionAnchor<object> } });
			return;
		}
		if (anchors.TryAdd(key, newAnchor as RedirectionAnchor<object>)) {
			return;
		}
		anchors[key] = newAnchor as RedirectionAnchor<object>;
	}
	
	internal RedirectionAnchor<T>? GetRedirectionAnchor<T>(string key = "anchor") {
		if(!registeredAnchors.TryGetValue(typeof(T), out var anchors))
			return null;
		if(!anchors.TryGetValue(key, out var anchor))
			return null;
		return anchor as RedirectionAnchor<T>;
	}
}


internal partial class RedirectionAnchor<ChildT> : ObservableObject {

	[ObservableProperty]
	internal ChildT? currentModel;

	public event Action ModelChanged = () => { };

	private readonly List<ChildT> models;

	internal RedirectionAnchor(List<ChildT> models) {
		this.models = models;
		if(models.Count > 0)
			CurrentModel = models[0];
	}

	internal bool IsActive<T>() =>
		typeof(T) == CurrentModel?.GetType();

	internal void ChangeModel<T>() {
		CurrentModel = models.First(x => x?.GetType() == typeof(T));
		ModelChanged.Invoke();
	}
}