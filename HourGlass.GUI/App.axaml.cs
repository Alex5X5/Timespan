namespace Hourglass.GUI;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Hourglass.Database.Services;
using Hourglass.Database.Services.Interfaces;
using Hourglass.GUI.Services;
using Hourglass.GUI.ViewModels;
using Hourglass.GUI.ViewModels.Components;
using Hourglass.GUI.ViewModels.Components.GraphPanels;
using Hourglass.GUI.ViewModels.Pages;
using Hourglass.GUI.ViewModels.Pages.SettingsPages;
using Hourglass.GUI.Views;
using Hourglass.PDF;
using Hourglass.PDF.Services.Interfaces;
using Hourglass.Util.Services;
using Microsoft.Extensions.DependencyInjection;

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
        instanciator.AddCommonServiceSingleton<ColorService, ColorService>();

		if (!Design.IsDesignMode) {
			HourglassDbService dbService = new();
			instanciator.AddCommonServiceSingleton<IHourglassDbService, HourglassDbService>(dbService);
			Services.CacheService cacheService = new(dbService);
			instanciator.AddCommonServiceSingleton<Services.CacheService, Services.CacheService>(cacheService);
			instanciator.AddCommonServiceSingleton<Util.Services.CacheService, Services.CacheService>(cacheService);
			instanciator.AddCommonServiceSingleton<IPdfService, PdfService>();
        }

		instanciator.RegisterComponentTransient<TaskGraphViewModel>();

		//instanciator.RegisterComponentTransient<DocumentPreviewerViewModel>();

		instanciator.AddContentBindingType<PageViewModelBase>();
		instanciator.RegisterPageSingleton<MainViewModel>();
		instanciator.RegisterPageTransient<TimerPageViewModel>();
		instanciator.RegisterPageTransient<ExportPageViewModel>();
		instanciator.RegisterPageTransient<ProjectPageViewModel>();
		instanciator.RegisterPageTransient<TaskDetailsPageViewModel>();

		instanciator.AddContentBindingType<GraphPanelViewModelBase>();
		instanciator.RegisterPageSingleton<GraphPageViewModel>();
		instanciator.RegisterPageTransient<DayGraphPanelViewModel>();
		instanciator.RegisterPageTransient<WeekGraphPanelViewModel>();
		instanciator.RegisterPageTransient<MonthGraphPanelViewModel>();

		instanciator.AddContentBindingType<SubSettingsPageViewModelBase>();
        instanciator.RegisterPageSingleton<SettingsPageViewModel>();
		instanciator.RegisterPageTransient<GeneralSubSettingsPageViewModel>();
		instanciator.RegisterPageTransient<AboutSubSettingsPageViewModel>();
		instanciator.RegisterPageTransient<ExportSubSettingsPageViewModel>();
		instanciator.RegisterPageTransient<VisualsSubSettingsPageViewModel>();
		instanciator.RegisterPageTransient<UserDataSubSettingsPageViewModel>();

		//instanciator.AddScopeController<SettingsPageViewModel>();

        var services = instanciator.BuildPages();

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			desktop.MainWindow = new MainWindow() {
				DataContext = services.GetRequiredService<MainViewModel>(),
				Icon = new WindowIcon(new Avalonia.Media.Imaging.Bitmap(PathService.AssetsPath("HourgalssIcon4.png")))
			};
		} else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
			singleViewPlatform.MainView = new MainView() {
				DataContext = services.GetRequiredService<MainViewModel>()
			};
		}

		base.OnFrameworkInitializationCompleted();
	}

    public static void SetTheme(string themeName) {
        var mergedDicts = Application.Current!.Resources.MergedDictionaries;

        var existing = mergedDicts.OfType<ResourceInclude>()
            .FirstOrDefault(r => r.Source?.OriginalString.Contains("Theme") == true);
        if (existing != null)
            mergedDicts.Remove(existing);

        var uri = new Uri($"avares://Hourglass.GUI/Assets/Themes/{themeName}Theme.axaml");
        mergedDicts.Add(new ResourceInclude(uri) { Source = uri });
    }
}

