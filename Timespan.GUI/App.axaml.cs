namespace Timespan.GUI;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Timespan.Database.Services;
using Timespan.Database.Services.Interfaces;
using Timespan.PDF.Services;
using Timespan.PDF.Services.Interfaces;
using Timespan.Util.Services;

using Microsoft.Extensions.DependencyInjection;

using Timespan.GUI.Services;
using Timespan.GUI.ViewModels;
using Timespan.GUI.Views;
using Timespan.GUI.Views.Graphs;
using Timespan.GUI.Views.Settings;

public partial class App : Application {

	public static App Current => (App)Application.Current;

    public IServiceProvider Services { private set; get; }

	public override void Initialize() {
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted() {
		PageInstanciator instanciator = new(this);

		instanciator.AddCommonServiceSingleton<CacheService, CacheService>();
		instanciator.AddCommonServiceSingleton<SettingsService, SettingsService>();
		instanciator.AddCommonServiceSingleton<RedirectionService, RedirectionService>();
		instanciator.AddCommonServiceSingleton<GuiStateService, GuiStateService>();
		instanciator.AddCommonServiceSingleton<ColorService, ColorService>();

		if (!Design.IsDesignMode) {
			TimespanDbService dbService = new();
			instanciator.AddCommonServiceSingleton<ITimespanDbService, TimespanDbService>(dbService);
			instanciator.AddCommonServiceSingleton<IPdfService, PdfService>();
		}

		instanciator.RegisterWindow<MainWindow>();

		instanciator.RegisterViewTransient<TaskDetailsView>();
		instanciator.RegisterViewTransient<ColorSelectorView>();

		instanciator.AddContentBindingType<IGraphsViewChild>();
		instanciator.RegisterViewTransient<DayPanelView>();
		instanciator.RegisterViewTransient<WeekPanelView>();
		instanciator.RegisterViewTransient<MonthPanelView>();

		instanciator.AddContentBindingType<IMainViewChild>();
		instanciator.RegisterViewTransient<TimerView>();
		instanciator.RegisterViewSingleton<GraphsView>();
		instanciator.RegisterViewTransient<ExportView>();
		instanciator.RegisterViewSingleton<MainView>();

		instanciator.AddScopedContentBindingType<ISettingsViewChild>();
		instanciator.RegisterViewScoped<GeneralSettingsView>();
		instanciator.RegisterViewScoped<UserDataSettingsView>();
		instanciator.RegisterViewScoped<AboutSettingsView>();
		instanciator.RegisterViewScoped<GraphicsSettingsView>();
		instanciator.RegisterViewScoped<ExportSettingsView>();
		instanciator.RegisterViewSingleton<SettingsView>();


		Services = instanciator.BuildPages();

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			desktop.MainWindow = new MainWindow() {
				DataContext = Services.GetRequiredService<MainViewModel>(),
				Title = "Timespan",
				Icon = new WindowIcon(new Avalonia.Media.Imaging.Bitmap(PathService.AssetsPath("HourgalssIcon4.png")))
			};
		} else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
			singleViewPlatform.MainView = new MainView() {
				DataContext = Services.GetRequiredService<MainViewModel>()
			};
		}

		base.OnFrameworkInitializationCompleted();
	}
}

