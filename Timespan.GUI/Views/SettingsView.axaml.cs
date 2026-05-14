namespace Timespan.GUI.Views;

public partial class SettingsView : UserControl
{

	public static new readonly StyledProperty<object?> ContentProperty =
		AvaloniaProperty.Register<SettingsView, object?>(nameof(Content));
	
	public new object? Content {
		get => GetValue(ContentProperty);
		set => SetValue(ContentProperty, value);
	}

	public SettingsView()
    {
        InitializeComponent();
    }
}