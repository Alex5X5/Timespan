namespace Timespan.GUI.Services;

using System;

using Timespan.GUI.ViewModels;

public class PageViewFactory(Func<Type, ViewModelBase> factory) {
	public ViewModelBase GetPageViewModel<T>(Action<T>? afterCreation = null)
		where T : ViewModelBase {
		var viewModel = factory(typeof(T));
		afterCreation?.Invoke((T)viewModel);
		return viewModel;
	}
}

//public class GraphPanelViewModelFactory(Func<Type, GraphPanelViewModelBase> factory) {
//	public GraphPanelViewModelBase GetGraphPanelViewModel<T>(Action<T>? afterCreation = null)
//		where T : GraphPanelViewModelBase {
//		var viewModel = factory(typeof(T));
//		afterCreation?.Invoke((T)viewModel);
//		return viewModel;
//	}
//}

