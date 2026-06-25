//namespace Timespan.GUI.MarkupExtensions;

//using Avalonia.Markup.Xaml;

//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Text;


//public class ViewModelExtension : MarkupExtension {

//	public override object ProvideValue(IServiceProvider serviceProvider) {
//		var provideTarget = serviceProvider
//			.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

//		if (provideTarget?.TargetObject is not Control control)
//			return null;

//		// We can't walk the tree yet — it's not attached during XAML parsing.
//		// So we defer until the control is added to the visual tree.
//		control.AttachedToVisualTree += OnAttached;
//		return null; // temporary null; real value set in OnAttached
//	}

//	private void OnAttached(object sender, VisualTreeAttachmentEventArgs e) {
//		var control = (Control)sender;
//		control.AttachedToVisualTree -= OnAttached;

//		// Walk up to the parent control
//		var parent = control.Parent as Control;
//		var parentVm = parent?.DataContext;
//		if (parentVm == null)
//			return;

//		// Find "ModelProvider" on the parent VM
//		var providerProp = parentVm.GetType()
//			.GetProperty("ModelProvider", BindingFlags.Public | BindingFlags.Instance);
//		var modelProvider = providerProp?.GetValue(parentVm);
//		if (modelProvider == null)
//			return;

//		// Call GetModel<TControl>() where TControl is the type of the control
//		var method = modelProvider.GetType()
//			.GetMethod("GetModel")
//			?.MakeGenericMethod(control.GetType());

//		var model = method?.Invoke(modelProvider, null);
//		control.DataContext = model;
//	}
//}