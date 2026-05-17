namespace Hourglass.GUI.ViewModels.Pages;

using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Hourglass.GUI.Services;
using Hourglass.PDF;
using Timespan.PDF.Services.Interfaces;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using static Hourglass.GUI.ViewModels.MainViewModel;

public partial class ExportPageViewModel : MainViewChildPageViewModel, INotifyPropertyChanged {

	private readonly DateTimeService? dateTimeService;
	CacheService cacheService;
	private readonly IPdfService? pdf;
	private readonly MainViewModel pageController;

    public override string Title => TranslatorService.Singleton["Views.Pages.Export.Title"] ?? "Export";

    private PdfDocumentData? pdfData;

    public PdfDocumentData? PdfData {
        set {
            pdfData = value;
            OnPropertyChanged(nameof(JobNameText));
            OnPropertyChanged(nameof(UseNameText));
            OnPropertyChanged(nameof(DateFromText));
            OnPropertyChanged(nameof(DateToText));
            OnPropertyChanged(nameof(WeekCount));
            OnPropertyChanged(nameof(TotalTime));
            OnPropertyChanged(nameof(MissingDays));
            OnPropertyChanged(nameof(SickDays));
            OnPropertyChanged(nameof(TotalMissingDays));
        }
        get => pdfData;
    }
    public ObservableCollection<TextboxItem> TableItems { get; set; }

	public string JobNameText => pdfData?.JobName ?? "";
	public string UseNameText => pdfData?.UserName ?? "";

	public string DateFromText => pdfData?.DateFrom ?? "";
	public string DateToText => pdfData?.DateTo ?? "";
	public string WeekCount => pdfData?.Week ?? "";

	public string TotalTime => pdfData?.TotalTime ?? "";


	public string MissingDays => pdfData?.MissingDays ?? "";
    public string SickDays => pdfData?.SickDays ?? "";
	public string TotalMissingDays => pdfData?.TotalMissingDays ?? "";

    private Action<Database.Models.Task> OnTextBockClick;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public ExportPageViewModel() : this(null) {
		
	}

	public ExportPageViewModel(DateTimeService? dateTimeService) : this(dateTimeService, null, null, null) {
		this.dateTimeService = dateTimeService;
	}

	public ExportPageViewModel(DateTimeService? dateTimeService, IPdfService? pdf, MainViewModel pageController, CacheService cacheService) : base() {
		this.dateTimeService = dateTimeService;
		this.pdf = pdf;
		this.pageController = pageController;
		this.cacheService = cacheService;
        OnTextBockClick =
			t => {
				cacheService.SelectedTask = t;
				pageController.GoToTaskdetails(t);
			};
		TableItems = [];
        Dispatcher.UIThread.InvokeAsync(
            () => {
                PdfData = pdf?.GetExportData(cacheService.SelectedDay) ?? new PdfDocumentData();
                for (int day = 0; day < 5; day++)
                    for (int i = 0; i < PdfDocumentData.DAY_LINE_COUNT; i++) {
                        int line = day * PdfDocumentData.DAY_LINE_COUNT + i;
                        TableItems.Add(new DescriptionItem { RowIndex = line, Text = PdfData.Data[line].Item1, Task = PdfData.Data[line].Item4 });
                        TableItems.Add(new HourItem { RowIndex = line, Text = PdfData.Data[line].Item2, Task = PdfData.Data[line].Item4 });
                        TableItems.Add(new HourRangeItem { RowIndex = line, Text = PdfData.Data[line].Item3, Task = PdfData.Data[line].Item4 });
                    }
            }
        );
    }

    private void SetTabButtonSeleted(int index) {
        foreach (var action in ButtonActions)
            action.Selected = action == ButtonActions[index];
    }

    protected void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    [RelayCommand]
	private async void Import() {
		//var topLevel = TopLevel.GetTopLevel();

		//// Start async operation to open the dialog.
		//var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
		//	Title = "Open Text File",
		//	AllowMultiple = false
		//});
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

    public void OnTaskRedirect(Database.Models.Task task) {
		cacheService.SelectedTask = task;
		pageController.ChangePage<TaskDetailsPageViewModel>();
		Console.WriteLine($"redirect event for task {task}");
	}
}

public abstract partial class TextboxItem {
	public int RowIndex { get; set; } = 0;

	public abstract int ColumnIndex { get; }

	public string Text { get; set; } = "";

	public Database.Models.Task Task {
		set;
		get;
	}
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