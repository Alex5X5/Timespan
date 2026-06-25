namespace Timespan.GUI.Services;

using System.Linq;
using System.Reflection;

public class ComponentModelFactory<ComponentT>(Func<ComponentT> factory) {

	public ComponentT GetComponentViewModel(
		Action<ComponentT?>? afterCreation = null,
		Dictionary<string, object?>? data = null
	) {
		ComponentT viewModel = factory();
		if (data != null) {
			PropertyInfo[] properties = typeof(ComponentT).GetProperties();
			FieldInfo[] fields = typeof(ComponentT).GetFields();
			foreach (string key in data.Keys) {
				PropertyInfo? property = properties.FirstOrDefault(x => x.Name == key);
				if (property != null) {
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