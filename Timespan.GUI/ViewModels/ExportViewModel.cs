namespace Timespan.GUI.ViewModels;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

using Timespan.GUI.Services;
using Timespan.GUI.Types.Events;
using Timespan.PDF.Services.Interfaces;
using Timespan.Types.Models;
using Timespan.Types.Pdf;
using Timespan.Util.Services;

using Types = Timespan.Types.Models;

public partial class ExportViewModel : ViewModelBase, IMainViewChild {

	private readonly RedirectionService redirectionService;
	private readonly Services.CacheService cacheService;
	private readonly IPdfService? pdf;

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

	public ExportViewModel(IPdfService? pdf, Services.CacheService cacheService, RedirectionService redirectionService) : base() {
		this.redirectionService = redirectionService;
		this.pdf = pdf;
		this.cacheService = cacheService;
		TableItems = [];
		Dispatcher.UIThread.Invoke(
			() => {
				var data = pdf?.GetExportData(cacheService.SelectedDay) ?? new PdfDocumentData();
				for (int day = 0; day < 5; day++)
					for (int i = 0; i < PdfDocumentData.DAY_LINE_COUNT; i++) {
						int line = day * PdfDocumentData.DAY_LINE_COUNT + i;
						PdfDocumentLine entry = data.Data[line];
						TableItems.Add(new DescriptionItem(entry.Task, line, entry.Description));
						TableItems.Add(new HourItem(entry.Task, line, entry.Hours));
						TableItems.Add(new HourRangeItem(entry.Task, line, entry.HourRange));
					}
			}
		);
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
		Console.WriteLine($"redirect event for task {task}");
		GlobalEventService.Raise(new ShowTaksEventArgs(task));
	}

	public void OnLoad() {
		Dispatcher.UIThread.Invoke(
			() => {
				var data = pdf?.GetExportData(cacheService.SelectedDay) ?? new PdfDocumentData();
				JobNameText = data.JobName;
				UserNameText = data.UserName;
				DateFromText = data.DateFrom;
				DateToText = data.DateTo;
				WeekCount = data.Week;
				SickDays = data.SickDays;
				MissingDays = data.MissingDays;
				TotalMissingDays = data.TotalMissingDays;
				TotalTime = data.TotalTime;
				TableItems = [];
				for (int day = 0; day < 5; day++)
					for (int i = 0; i < PdfDocumentData.DAY_LINE_COUNT; i++) {
						int line = day * PdfDocumentData.DAY_LINE_COUNT + i;
						PdfDocumentLine entry = data.Data[line];
						TableItems.Add(new DescriptionItem(entry.Task, line, entry.Description));
						TableItems.Add(new HourItem(entry.Task, line, entry.Hours));
						TableItems.Add(new HourRangeItem(entry.Task, line, entry.HourRange));
					}
				OnPropertyChanged(nameof(TableItems));
			}
		);
	}
}

public abstract record TextboxItem(int ColumnIndex, Types.Task? Task, int RowIndex = 0, string Text = "");

public record DescriptionItem(Types.Task? Task, int RowIndex = 0, string Text = "")
	: TextboxItem(0, Task, RowIndex, Text);

public record HourItem(Types.Task? Task, int RowIndex = 0, string Text = "")
	: TextboxItem(1, Task, RowIndex, Text);

public record HourRangeItem(Types.Task? Task, int RowIndex = 0, string Text = "")
	: TextboxItem(2, Task, RowIndex, Text);