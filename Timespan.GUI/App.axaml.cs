namespace Timespan.GUI;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Hourglass.Database.Services;
using Hourglass.Database.Services.Interfaces;
using Hourglass.PDF;
using Hourglass.PDF.Services.Interfaces;
using Hourglass.Util.Services;

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

		PathService.PrintDetailedInfo();
		PathService.ExtractFiles("Hourglass");

		PageInstanciator instanciator = new(this);

		instanciator.AddCommonServiceSingleton<DateTimeService, DateTimeService>();
		instanciator.AddCommonServiceSingleton<SettingsService, SettingsService>();
		instanciator.AddCommonServiceSingleton<RedirectionService, RedirectionService>();

		if (!Design.IsDesignMode) {
			HourglassDbService dbService = new();
			instanciator.AddCommonServiceSingleton<IHourglassDbService, HourglassDbService>(dbService);
			Services.CacheService cacheService = new(dbService);
			instanciator.AddCommonServiceSingleton<Services.CacheService, Services.CacheService>(cacheService);
			instanciator.AddCommonServiceSingleton<Hourglass.Util.Services.CacheService, Services.CacheService>(cacheService);
			instanciator.AddCommonServiceSingleton<IPdfService, PdfService>();
		}

		instanciator.RegisterWindow<MainWindow>();

		instanciator.AddContentBindingType<IGraphsViewChild>();
		instanciator.RegisterPageTransient<DayViewModel>();
		instanciator.RegisterPageTransient<WeekViewModel>();
		instanciator.RegisterPageTransient<MonthViewModel>();

		instanciator.AddContentBindingType<IMainViewChild>();
		instanciator.RegisterPageTransient<TimerViewModel>();
		instanciator.RegisterPageSingleton<GraphsViewModel>();
		instanciator.RegisterPageTransient<ExportViewModel>();
		instanciator.RegisterPageSingleton<MainViewModel>();

		instanciator.AddContentBindingType<ISettingsViewChild>();
		instanciator.RegisterPageTransient<GeneralSettingsViewModel>();
		instanciator.RegisterPageTransient<UserDataSettingsViewModel>();
		instanciator.RegisterPageTransient<AboutSettingsViewModel>();
		instanciator.RegisterPageTransient<GraphicsSettingsViewModel>();
		instanciator.RegisterPageTransient<ExportSettingsViewModel>();
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

