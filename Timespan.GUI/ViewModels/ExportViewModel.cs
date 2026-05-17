namespace Timespan.GUI.ViewModels;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Timespan.PDF.Services.Interfaces;
using Timespan.PDF.Services;
using Timespan.Util.Services;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

using Timespan.GUI.Services;
using Timespan.GUI.ViewModels.Graphs;

using Types = Timespan.Types.Models;

public partial class ExportViewModel : ViewModelBase, IMainViewChild {

	private readonly RedirectionService redirectionService;
	private readonly DateTimeService? dateTimeService;
	private readonly Services.CacheService cacheService;
	private readonly IPdfService? pdf;
	private readonly MainViewModel pageController;

	public ObservableCollection<TextboxItem> TableItems {
		get; set;
	}

	[ObservableProperty]
	public string jobNameText;

	[ObservableProperty]
	public string userNameText;

	[ObservableProperty]
	public string dateFromText;
	
	[ObservableProperty]
	public string dateToText;

	[ObservableProperty]
	public string weekCount;

	[ObservableProperty]
	public string totalTime;

	[ObservableProperty]
	public string missingDays;
	
	[ObservableProperty]
	public string sickDays;
	
	[ObservableProperty]
	public string totalMissingDays;

	public ExportViewModel(DateTimeService? dateTimeService, IPdfService? pdf, Services.CacheService cacheService, RedirectionService redirectionService) : base() {
		this.redirectionService = redirectionService;
		this.dateTimeService = dateTimeService;
		this.pdf = pdf;
		this.cacheService = cacheService;
		TableItems = [];
	}

	[RelayCommand]
	private async void Import() {
		Console.WriteLine("import button click! (not yet implemented)");
	}

	[RelayCommand]
	private void Export() {
		Console.WriteLine("export button click!");
		new Thread(
			() => {
				pdf?.Export(cacheService.SelectedDay);
			}
		).Start();
	}

	[RelayCommand]
	private void OpenExplorer() {
		string folderPath = PathService.FilesPath(@"Nachweise\");
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			Process.Start("explorer.exe", $"{folderPath}");
		} else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
			Process.Start("open", $"{folderPath}");
		} else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
			Process.Start("xdg-open", $"{folderPath}");
		}
	}

	public void OnTaskRedirect(Types.Task task) {
		cacheService.SelectedTask = task;
		redirectionService.GetAnchor<GraphsViewModel, IGraphsViewChild>()?.ChangeModel<DayViewModel>();
		redirectionService.GetAnchor<MainViewModel, IMainViewChild>()?.ChangeModel<GraphsViewModel>(x=>x?.ShowTask());
		Console.WriteLine($"redirect event for task {task}");
	}

	public void OnLoad() {
		Dispatcher.UIThread.InvokeAsync(
			() => {
				var data = pdf?.GetExportData(cacheService.SelectedDay) ?? new PdfDocumentData();
				JobNameText = data.JobName;
				UserNameText = data.UserName;
				DateFromText = data.DateFrom;
				DateToText = data.DateTo;
				WeekCount = data.Week;
				TableItems = [];
				for (int day = 0; day < 5; day++)
					for (int i = 0; i < PdfDocumentData.DAY_LINE_COUNT; i++) {
						int line = day * PdfDocumentData.DAY_LINE_COUNT + i;
						TableItems.Add(new DescriptionItem { RowIndex = line, Text = data.Data[line].Item1, Task = data.Data[line].Item4 });
						TableItems.Add(new HourItem { RowIndex = line, Text = data.Data[line].Item2, Task = data.Data[line].Item4 });
						TableItems.Add(new HourRangeItem { RowIndex = line, Text = data.Data[line].Item3, Task = data.Data[line].Item4 });
					}
				OnPropertyChanged(nameof(TableItems));
			}
		);
	}
}

public abstract class TextboxItem {
	public int RowIndex { get; set; } = 0;

	public abstract int ColumnIndex { get; }

	public string Text { get; set; } = "";

	public Types.Task Task { set; get; }
}

public class DescriptionItem : TextboxItem {
	public override int ColumnIndex => 0;

}

public class HourItem : TextboxItem {
	public override int ColumnIndex => 1;

}

public class HourRangeItem : TextboxItem {
	public override int ColumnIndex => 2;
}