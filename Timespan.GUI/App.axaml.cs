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
using Timespan.GUI.ViewModels.Graphs;
using Timespan.GUI.ViewModels.Settings;
using Timespan.GUI.Views;

public partial class App : Application {

	public override void Initialize() {
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted() {
		PageInstanciator instanciator = new(this);

		instanciator.AddCommonServiceSingleton<DateTimeService, DateTimeService>();
		instanciator.AddCommonServiceSingleton<SettingsService, SettingsService>();
		instanciator.AddCommonServiceSingleton<RedirectionService, RedirectionService>();

		if (!Design.IsDesignMode) {
			HourglassDbService dbService = new();
			instanciator.AddCommonServiceSingleton<IHourglassDbService, HourglassDbService>(dbService);
			Services.CacheService cacheService = new();
			cacheService.RunningTask = dbService.QueryCurrentTaskAsync().Result;
			instanciator.AddCommonServiceSingleton<Services.CacheService, Services.CacheService>(cacheService);
			instanciator.AddCommonServiceSingleton<Timespan.Util.Services.CacheService, Services.CacheService>(cacheService);
			instanciator.AddCommonServiceSingleton<IPdfService, PdfService>();
		}

		instanciator.RegisterWindow<MainWindow>();

		instanciator.AddContentBindingType<IGraphsViewChild>();
		instanciator.RegisterPageTransient<DayPanelViewModel>();
		instanciator.RegisterPageTransient<WeekPanelViewModel>();
		instanciator.RegisterPageTransient<MonthPanelViewModel>();

		instanciator.AddContentBindingType<IMainViewChild>();
		instanciator.RegisterPageTransient<TimerViewModel>();
		instanciator.RegisterPageSingleton<GraphsViewModel>();
		instanciator.RegisterPageTransient<ExportViewModel>();
		instanciator.RegisterPageSingleton<MainViewModel>();

		instanciator.AddScopedContentBindingType<ISettingsViewChild>();
		instanciator.RegisterPageScoped<GeneralSettingsViewModel>();
		instanciator.RegisterPageScoped<UserDataSettingsViewModel>();
		instanciator.RegisterPageScoped<AboutSettingsViewModel>();
		instanciator.RegisterPageScoped<GraphicsSettingsViewModel>();
		instanciator.RegisterPageScoped<ExportSettingsViewModel>();
		instanciator.RegisterPageSingleton<SettingsViewModel>();


		var services = instanciator.BuildPages();

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			desktop.MainWindow = new MainWindow() {
				DataContext = services.GetRequiredService<MainViewModel>(),
				Title = "Timespan",
				Icon = new WindowIcon(new Avalonia.Media.Imaging.Bitmap(PathService.AssetsPath("HourgalssIcon4.png")))
			};
		} else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
			singleViewPlatform.MainView = new MainView() {
				DataContext = services.GetRequiredService<MainViewModel>()
			};
		}

		base.OnFrameworkInitializationCompleted();
	}
}

