namespace Timespan.GUI.Services;

using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class PageInstanciator {

	private readonly IServiceCollection serviceCollection = new ServiceCollection();

	public PageInstanciator(Application application) : this() {
		serviceCollection.AddSingleton<Func<TopLevel?>>(
			provider => 
				() => {
					if (application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime topWindow)
						return TopLevel.GetTopLevel(topWindow.MainWindow);
					if (application.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
						return TopLevel.GetTopLevel(singleViewPlatform.MainView);
					return null;
				}
		);
	}

	public PageInstanciator() {
	}

	public void AddCommonServiceSingleton(Type serviceType) {
		serviceCollection.AddSingleton(serviceType);
	}

	public void AddCommonServiceSingleton<RegisterT, InstanceT>(InstanceT singleton)
		where RegisterT : class
		where InstanceT : class, RegisterT {
		serviceCollection.AddSingleton<RegisterT>(singleton);
	}

	public void AddCommonServiceSingleton<RegisterT, InstanceT>() 
		where RegisterT : class 
		where InstanceT : class, RegisterT {
		serviceCollection.AddSingleton<RegisterT, InstanceT>();
	}

	public void RegisterWindow<T>() where T : Window {
		serviceCollection.AddSingleton<T>();
	}

	public void RegisterPageSingleton<T>() where T : class {
		serviceCollection.AddSingleton<T>();
	}

	public void RegisterPageTransient<T>() where T : class {
		serviceCollection.AddTransient<T>();
	}

	public void RegisterPageScoped<T>() where T : class {
		serviceCollection.AddScoped<T>();
	}

	public void RegisterComponentTransient<ComponentT>() where ComponentT : class {
		serviceCollection.AddTransient<ComponentT>();
		serviceCollection.AddSingleton<Func<ComponentT>>(
			(serviceProvider) => 
				() => serviceProvider.GetService<ComponentT>() ?? Activator.CreateInstance<ComponentT>()
		);
        serviceCollection.AddSingleton<ComponentModelFactory<ComponentT>>();
    }

	public void AddContentBindingType<ContentBaseT>() {
		serviceCollection.AddSingleton<Func<Type, ContentBaseT>>(
			(serviceProvider) =>
				(pageType) =>
					(ContentBaseT?)serviceProvider.GetService(pageType)
						?? throw new InvalidOperationException($"View of type {pageType?.FullName} has no registered view model")
		);
        serviceCollection.AddSingleton<ViewModelFactory<ContentBaseT>>();
	}

	public void AddScopedContentBindingType<ContentBaseT>() {
		serviceCollection.AddSingleton<Func<Type, IServiceScope, ContentBaseT>>(
			(serviceProvider) =>
				(pageType, scope) =>
					(ContentBaseT?)scope.ServiceProvider.GetService(pageType)
						?? throw new InvalidOperationException($"View of type {pageType?.FullName} has no registered view model")
		);
		serviceCollection.AddSingleton<ScopedViewModelFactory<ContentBaseT>>();
	}


	public IServiceProvider BuildPages() {
        return serviceCollection.BuildServiceProvider();
    }
}

public class ViewModelFactory<ViewBaseType>(Func<Type, ViewBaseType> factory) {
	public ViewBaseType BuildViewModel<T>(Action<T?>? afterCreation = null)
		where T : ViewBaseType {
		ViewBaseType viewModel = factory(typeof(T));
		afterCreation?.Invoke((T?)viewModel);
		return viewModel;
	}
}

public class ScopedViewModelFactory<ViewBaseType>(Func<Type, IServiceScope, ViewBaseType> factory) {
	public ViewBaseType BuildViewModel<T>(IServiceScope scope, Action<T?>? afterCreation = null)
		where T : ViewBaseType {
		ViewBaseType viewModel = factory(typeof(T), scope);
		afterCreation?.Invoke((T?)viewModel);
		return viewModel;
	}
}

public class ComponentModelFactory<ComponentT>(Func<ComponentT> factory) {

    public ComponentT GetComponentViewModel(
		Action<ComponentT?>? afterCreation = null,
		Dictionary<string, object?>? data = null
	) {
        ComponentT viewModel = factory();
		if(data != null) {
			PropertyInfo[] properties = typeof(ComponentT).GetProperties();
			FieldInfo[] fields = typeof(ComponentT).GetFields();
            foreach (string key in data.Keys) {
				PropertyInfo? property = properties.FirstOrDefault(x => x.Name == key);
				if(property != null) {
					property?.SetValue(viewModel, data[key]);
					continue;
				}
				fields.FirstOrDefault(x => x.Name == key)?.SetValue(viewModel, data[key]);
			}
		}
        afterCreation?.Invoke(viewModel);
        return viewModel;
    }
}
