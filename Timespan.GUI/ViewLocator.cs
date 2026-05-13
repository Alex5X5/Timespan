namespace Timespan.GUI;

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using System;
using Timespan.GUI.ViewModels;

public class ViewLocator : IDataTemplate {

	public Control Build(object? data) {
		if (data is null) {
			return new TextBlock { Text = "data was null" };
		}

		var viewTypeName = data.GetType().FullName!.Replace("ViewModel", "View");
		var viewType = Type.GetType(viewTypeName);
		var modelType = data.GetType();
		if (viewType != null) {
			if (!modelType.IsSubclassOf(typeof(ViewModelBase)))
				return new TextBlock { Text = $"{modelType.Name} is not a view model type"};
			Control res;
			
			if (data is ViewModelBase model) {
				res = (Control)Activator.CreateInstance(viewType)!;
				res.DataContext = model;
				return res;
			}

			res = (Control)Activator.CreateInstance(viewType)!;
			res.DataContext = Activator.CreateInstance(modelType);
			return res;
		} else {
			return new TextBlock { Text = "Not Found: " + viewTypeName };
		}
	}

	public bool Match(object? data) {
		return data is ViewModelBase;
	}
}