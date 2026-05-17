namespace Timespan.PDF.Services;

using Timespan.Database.Models;
using Timespan.Database.Services.Interfaces;
using Timespan.PDF.Services.Interfaces;
using Timespan.Util.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

public unsafe partial class PdfService : IPdfService, IDisposable {

	private readonly IHourglassDbService _dbService;
	private SettingsService settingsService;
	DateTimeService dateTimeService;
	CacheService cacheService;

	public const int MAX_LINE_LENGTH = 85;

	private const string LAST_SECTION_INDEXER = "eof";

	public static bool IndexersLoaded { private set; get; } = false;

	private readonly Dictionary<string, ValueTuple<IntPtr, IntPtr>> Indexers;

	public Dictionary<string, string> InsertOperations;

	private readonly int charCount;
	private readonly char* text;

	public PdfService(IHourglassDbService dbService, SettingsService settingsService, DateTimeService dateTimeService, CacheService cacheService) {
		this.dateTimeService = dateTimeService;
		this.settingsService = settingsService;
		this.cacheService = cacheService;
		_dbService = dbService;
		InsertOperations = [];
		Indexers = [];
		byte* buffer = FileService.LoadFileUnsafe(PathService.AssetsPath("output-readable-indexers.pdf"), out int inputFileSize);
		text = FileService.DecodeBufferAnsi(buffer, inputFileSize, out int _charCount);
		charCount = _charCount;
		NativeMemory.Free(buffer);
		new Thread(LoadIndexers).Start();
	}

	public void Dispose() {
		GC.SuppressFinalize(this);
		NativeMemory.Free(text);
	}

	private void PrintCharsBefore(char* ptr) {
		Console.Write($"chars before insert pos:");
		char* charBefore = ptr - 25;
		for (int i = 0; i < 25; i++) {
			Console.Write($"{*charBefore}");
			charBefore++;
		}
		Console.WriteLine();
	}

	private bool WaitForIndexing() {
		int counter = 0;
		while (counter < 10) {
			if (IndexersLoaded)
				return true;
			Thread.Sleep(200);
		}
		return false;
	}

	public void LoadIndexers() {
		Console.WriteLine("loading indexers");
		Stopwatch stopwatch = new();
		stopwatch.Start();
		int i = 0;
		char* _content = text;
		int annotationCount = 0;
		int fieldCount = 0;
		char* previouslastSectionCharacter = text;
		while (i < charCount) {
			if (_content[0] == '%' && _content[1] == '%' && _content[2] == 'i' && _content[3] == 'n' && _content[4] == 'd' && _content[5] == 'e' && _content[6] == 'x') {
				string key = Convert.ToString(*_content);
				while (true) {
					_content++;
					i++;
					if (*_content == ' ' | *_content == '\n' | *_content == '\r' | i >= charCount)
						break;
					key += Convert.ToString(*_content);
				}
				_content++;
				i++;
				if (_content[0] == '/' && _content[1] == 'V' && _content[2] == ' ' && _content[3] == '(' && _content[4] == ')') {
					annotationCount++;
					_content += 4;
					i += 4;
					Indexers[key] = ((IntPtr)previouslastSectionCharacter, (IntPtr)_content);
					previouslastSectionCharacter = _content;
				} else if (_content[0] == '(' && _content[1] == ')' && _content[2] == ' ' && _content[3] == 'T' && _content[4] == 'j') {
					fieldCount++;
					_content += 1;
					i += 1;
					Indexers[key] = ((IntPtr)previouslastSectionCharacter, (IntPtr)_content);
					previouslastSectionCharacter = _content;
				}
			}
			_content++;
			i++;
		}
		Indexers[LAST_SECTION_INDEXER] = ((IntPtr)previouslastSectionCharacter, (IntPtr)(text+charCount));
		IndexersLoaded = true;
		stopwatch.Stop();
		Console.WriteLine("finished loading indexers");
		Console.WriteLine($"loading indexers took {stopwatch.ElapsedMilliseconds / 1000.0} seconds");
	}

	public void Export(IProgressReporter progressReporter, DateTime selectedWeek) {
		if (!IndexersLoaded)
			return;
		Stopwatch totalStopwatch = new();
		totalStopwatch.Start();
		Console.WriteLine("started expoting");
		selectedWeek = DateTimeService.FloorWeek(selectedWeek);
		Stopwatch prepareContentStopwatch = new();
		prepareContentStopwatch.Start();
		Console.WriteLine("started preparing content for the document");
		List<Database.Models.Task> tasks = _dbService.QueryTasksOfWeekAtDateAsync(selectedWeek).Result;
		Dictionary<string, DayOfWeek> days = new Dictionary<string, DayOfWeek> {
			{ "monday", DayOfWeek.Monday },
			{ "tuesday", DayOfWeek.Tuesday },
			{ "wendsday", DayOfWeek.Wednesday },
			{ "thursday", DayOfWeek.Thursday },
			{ "friday", DayOfWeek.Friday }
		};
		long totalWeekSeconds = 0;
		string query = "";
		string value = "";
		const int progressUpdatesPerTask = 1;
		int totalSteps = tasks.Count * progressUpdatesPerTask;
		int currentStep = 0;
		int percentage = 0;
		foreach (string dayName in days.Keys) {
			int offset = 0;
			string[] lines = ["", "", "", "", "", ""];
			List<Database.Models.Task> tasks_ = tasks.Where(x => x.FinishDateTime.DayOfWeek == days[dayName]).ToList();
			if (tasks_.Count == 0)
				continue;
			foreach (Database.Models.Task task in tasks_) {
				if (progressReporter.IsCancellationRequested) {
					progressReporter.ReportProgress(currentStep, "Cancelling...");
					Thread.Sleep(500);
					return;
				}
				if (task.running)
					continue;
				string[] compiledTask = CompileTask(task);
				try {
					Array.ConstrainedCopy(compiledTask, 0, lines, offset, compiledTask.Length);
					query = $"{dayName}_hour_range_{offset + 1}";
					value = DateTimeService.ToTimeString(task.StartDateTime) + " - " + DateTimeService.ToTimeString(task.FinishDateTime);
					BufferAnnotationValueUnsafe(query, value);
					BufferFieldValueUnsafe(query, value);
					query = $"{dayName}_hour_{offset + 1}";
					value = DateTimeService.ToHourMinuteString(task.finish-task.start);
					BufferAnnotationValueUnsafe(query, value);
					BufferFieldValueUnsafe(query, value);
					offset += compiledTask.Length;
				} catch (ArgumentOutOfRangeException) {
					Console.WriteLine($"ran out of empty lines while inserting {compiledTask.Length} lines for day {dayName}");
					Console.WriteLine($"description of task was:'{task.description}'");
					break;
				} catch (ArgumentException) {
					Console.WriteLine($"ran out of empty lines while inserting {compiledTask.Length} lines for day {dayName}");
					Console.WriteLine($"description of task was:'{task.description}'");
					break;
				}
				totalWeekSeconds += task.finish - task.start;
				currentStep++;
				percentage = (int)(currentStep * 100.0 / totalSteps);
				progressReporter.ReportProgress(percentage, $"Processing day {dayName}...");
			}
			for (int i = 0; i < lines.Length; i++) {
				query = $"{dayName}_line_{i + 1}";
				BufferAnnotationValueUnsafe(query, lines[i]);
				BufferFieldValueUnsafe(query, lines[i]);
			}
		}
		query = $"total_hour";
		value = DateTimeService.ToHourMinuteString(totalWeekSeconds);
		BufferAnnotationValueUnsafe(query, value);
		BufferFieldValueUnsafe(query, value);
		SetUtilityFields(selectedWeek);
		prepareContentStopwatch.Stop();
		Console.Write("finished preparing content for the document\n");
		Console.WriteLine($"preparing content took {prepareContentStopwatch.ElapsedMilliseconds / 1000.0} seconds");
		char* document = BuildDocument(out int documentCharCount);
		byte* resultFile = FileService.EncodeBufferAnsi(document, documentCharCount, out int fileSize);
		FileService.WriteFileUnsafe(resultFile, PathService.FilesPath($"Nachweise/{GetNewFileName(selectedWeek)}"), fileSize);
		NativeMemory.Free(resultFile);
		NativeMemory.Free(document);
		InsertOperations.Clear();
		totalStopwatch.Stop();
		Console.Write("finished exporting unsafe\n");
		Console.WriteLine($"exporting took {totalStopwatch.ElapsedMilliseconds / 1000.0} seconds");
		progressReporter.ReportProgress(100, "finished exporting");
	}



	public void Export(DateTime selectedWeek) {
		if (!IndexersLoaded)
			if(!WaitForIndexing())
				return;
		Stopwatch totalStopwatch = new();
		totalStopwatch.Start();
		Console.WriteLine("started expoting");
		selectedWeek = DateTimeService.FloorWeek(selectedWeek);
		Stopwatch prepareContentStopwatch = new();
		prepareContentStopwatch.Start();
		Console.WriteLine("started preparing content for the document");
		List<Database.Models.Task> tasks = _dbService.QueryTasksOfWeekAtDateAsync(selectedWeek).Result;
		Dictionary<string, DayOfWeek> days = new Dictionary<string, DayOfWeek> {
			{ "monday", DayOfWeek.Monday },
			{ "tuesday", DayOfWeek.Tuesday },
			{ "wendsday", DayOfWeek.Wednesday },
			{ "thursday", DayOfWeek.Thursday },
			{ "friday", DayOfWeek.Friday }
		};
		long totalWeekSeconds = 0;
		string query = "";
		string value = "";
		const int progressUpdatesPerTask = 1;
		int totalSteps = tasks.Count * progressUpdatesPerTask;
		int currentStep = 0;
		int percentage = 0;
		foreach (string dayName in days.Keys) {
			int offset = 0;
			string[] lines = ["", "", "", "", "", ""];
			List<Database.Models.Task> tasks_ = tasks.Where(x => x.StartDateTime.DayOfWeek == days[dayName]).ToList();
			if (tasks_.Count == 0)
				continue;
			foreach (Database.Models.Task task in tasks_) {
				if (task.running)
					continue;
				string[] compiledTask = CompileTask(task);
				try {
					Array.ConstrainedCopy(compiledTask, 0, lines, offset, compiledTask.Length);
					query = $"{dayName}_hour_range_{offset + 1}";
					value = DateTimeService.ToHourMinuteString(task.start) + " - " + DateTimeService.ToHourMinuteString(task.finish);
					BufferAnnotationValueUnsafe(query, value);
					BufferFieldValueUnsafe(query, value);
					query = $"{dayName}_hour_{offset + 1}";
					value = DateTimeService.ToHourMinuteString(task.finish - task.start);
					BufferAnnotationValueUnsafe(query, value);
					BufferFieldValueUnsafe(query, value);
					offset += compiledTask.Length;
				} catch (ArgumentOutOfRangeException) {
					Console.WriteLine($"ran out of empty lines while inserting {compiledTask.Length} lines for day {dayName}");
					Console.WriteLine($"description of task was:'{task.description}'");
					break;
				} catch (ArgumentException) {
					Console.WriteLine($"ran out of empty lines while inserting {compiledTask.Length} lines for day {dayName}");
					Console.WriteLine($"description of task was:'{task.description}'");
					break;
				}
				totalWeekSeconds += task.finish - task.start;
				currentStep++;
				percentage = (int)(currentStep * 100.0 / totalSteps);
			}
			for (int i = 0; i < lines.Length; i++) {
				query = $"{dayName}_line_{i + 1}";
				BufferAnnotationValueUnsafe(query, lines[i]);
				BufferFieldValueUnsafe(query, lines[i]);
			}
		}
		query = $"total_hour";
		value = DateTimeService.ToHourMinuteString(totalWeekSeconds);
		BufferAnnotationValueUnsafe(query, value);
		BufferFieldValueUnsafe(query, value);
		SetUtilityFields(selectedWeek);
		prepareContentStopwatch.Stop();
		Console.Write("finished preparing content for the document\n");
		Console.WriteLine($"preparing content took {prepareContentStopwatch.ElapsedMilliseconds / 1000.0} seconds");
		char* document = BuildDocument(out int documentCharCount);
		byte* resultFile = FileService.EncodeBufferAnsi(document, documentCharCount, out int fileSize);
		FileService.WriteFileUnsafe(resultFile, PathService.FilesPath($"Nachweise/{GetNewFileName(selectedWeek)}"), fileSize);
		NativeMemory.Free(resultFile);
		NativeMemory.Free(document);
		InsertOperations.Clear();
		totalStopwatch.Stop();
		Console.Write("finished exporting unsafe\n");
		Console.WriteLine($"exporting took {totalStopwatch.ElapsedMilliseconds / 1000.0} seconds");
	}

	public PdfDocumentData? GetExportData(DateTime selectedWeek) {
		if (!IndexersLoaded)
			if (!WaitForIndexing())
				return null;
		PdfDocumentData data = new PdfDocumentData();
		List<Database.Models.Task> tasks = _dbService.QueryTasksOfWeekAtDateAsync(selectedWeek).Result;
		Dictionary<string, DayOfWeek> days = new Dictionary<string, DayOfWeek> {
			{ "monday", DayOfWeek.Monday },
			{ "tuesday", DayOfWeek.Tuesday },
			{ "wednsday", DayOfWeek.Wednesday },
			{ "thursday", DayOfWeek.Thursday },
			{ "friday", DayOfWeek.Friday }
		};
		long totalWeekSeconds = 0;
		int dayCounter = 0;
		foreach (string dayName in days.Keys) {
			int offset = 0;
			string[] lines = ["", "", "", "", "", ""];
			Task[] lineTaks = new Task[lines.Length];
			string[] hours = ["", "", "", "", "", ""];
			string[] hourRanges = ["", "", "", "", "", ""];
			List<Task> tasks_ = tasks.Where(x => x.StartDateTime.DayOfWeek == days[dayName]).ToList();
            if (tasks_.Count == 0) {
				dayCounter++;
				continue;
			}
			DateTime date_ = DateTimeService.FloorDay(DateTimeService.FloorWeek(selectedWeek).AddDays(dayCounter));
            Task? blockedBy = _dbService.QueryBlockingTasksAtDateAsync(date_).Result.FirstOrDefault();
			if (blockedBy != null) {
				lines[0] = blockedBy.description;
			} else {
				foreach (Task task in tasks_) {
					if (task.running)
						continue;
					string[] compiledTask = CompileTaskPreview(task);
					try {
						Array.ConstrainedCopy(compiledTask, 0, lines, offset, compiledTask.Length);
						for (int i = 0; i < compiledTask.Length; i++)
							lineTaks[offset + i] = task;
						for (int i = 0; i < compiledTask.Length; i++)
							lineTaks[offset + i] = task;
						hourRanges[offset] = DateTimeService.ToHourMinuteString(task.start) + " - " + DateTimeService.ToHourMinuteString(task.finish);
						hours[offset] = DateTimeService.ToHourMinuteString(task.finish - task.start);
						offset += compiledTask.Length;
					} catch (ArgumentOutOfRangeException) {
						Console.WriteLine($"ran out of empty lines while inserting {compiledTask.Length} lines for day {dayName}");
						Console.WriteLine($"description of task was:'{task.description}'");
						break;
					} catch (ArgumentException) {
						Console.WriteLine($"ran out of empty lines while inserting {compiledTask.Length} lines for day {dayName}");
						Console.WriteLine($"description of task was:'{task.description}'");
						break;
					}
					totalWeekSeconds += task.finish - task.start;
				}
			}
			for (int i = 0; i < 6; i++) {
				data.Data[dayCounter * PdfDocumentData.DAY_LINE_COUNT + i].Item1 = lines[i];
				data.Data[dayCounter * PdfDocumentData.DAY_LINE_COUNT + i].Item2 = hours[i];
                data.Data[dayCounter * PdfDocumentData.DAY_LINE_COUNT + i].Item3 = hourRanges[i];
                data.Data[dayCounter * PdfDocumentData.DAY_LINE_COUNT + i].Item4 = lineTaks[i];
            }
			dayCounter++;
		}
		data.TotalTime = DateTimeService.ToHourMinuteString(totalWeekSeconds);
		int week = dateTimeService.GetWeekCountAtDate(selectedWeek);
        data.Week = Convert.ToString(week);
		data.UserName = settingsService.TryGetSetting(SettingsService.USER_NAME_KEY) ?? "username";
		data.JobName = settingsService.TryGetSetting(SettingsService.JOB_NAME_KEY) ?? "job name";
		bool[] missingDays = new bool[5000];
		long startDateSeconds = DateTimeService.ToSeconds(settingsService.StartDate);
		long maxDateSeconds = DateTimeService.ToSeconds(DateTimeService.CeilWeek(cacheService.SelectedDay));
		List<Task> blockingTasks = _dbService.QueryBlockingTasksInIntervallAsync(startDateSeconds, maxDateSeconds).Result;
        foreach (Task t in blockingTasks) {
			long taskStartOffsetSeconds = t.start - startDateSeconds;
			int taskStartOffsetDays = (int)Math.Floor((double)taskStartOffsetSeconds / TimeSpan.SecondsPerDay);
			if(t.blocksTime==Database.BlockedTimeIntervallType.Sick | t.blocksTime == Database.BlockedTimeIntervallType.NoExcuse)
				missingDays[taskStartOffsetDays] = true;
        }
		data.TotalMissingDays=Convert.ToString(missingDays.Count(c => c ==true));
        startDateSeconds = DateTimeService.ToSeconds(DateTimeService.FloorWeek(cacheService.SelectedDay));
        bool[] newMissingDays = new bool[7];
        bool[] newSickDays = new bool[7];
        foreach (Task t in blockingTasks.Where(x=>x.start>startDateSeconds).ToList()) {
            long taskStartOffsetSeconds = t.start - startDateSeconds;
            int taskStartOffsetDays = (int)Math.Floor((double)taskStartOffsetSeconds / TimeSpan.SecondsPerDay);
            if (t.blocksTime == Database.BlockedTimeIntervallType.NoExcuse)
                newMissingDays[taskStartOffsetDays] = true;
            if (t.blocksTime == Database.BlockedTimeIntervallType.Sick)
                newSickDays[taskStartOffsetDays] = true;
        }
        data.TotalMissingDays = Convert.ToString(missingDays.Count(c => c == true));
        data.MissingDays = Convert.ToString(newMissingDays.Count(c => c == true));
        data.SickDays = Convert.ToString(newSickDays.Count(c => c == true));
        DateTime dayFrom = DateTimeService.FloorWeek(selectedWeek);
        DateTime dayTo = dayFrom.AddDays(5);
		data.DateFrom = DateTimeService.ToDayAndMonthAndYearString(dayFrom);
		data.DateTo = DateTimeService.ToDayAndMonthAndYearString(dayTo);
        return data;
	}

	public void Import() {
		throw new NotImplementedException();
	}

	public static string[] CompileTask(Database.Models.Task task) {
		string source = "";
		List<string> res = [];
		if (task.project != null)
			source += $"{task.project.Name}: ";
		if (task.ticket != null)
			source += $"{task.ticket.name}: ";
		source += task.description;
		while (source.Length > 0) {
			int CharacterRemoveCount;
			if (source.Length >= MAX_LINE_LENGTH) {
				CharacterRemoveCount = MAX_LINE_LENGTH;
				while (source[CharacterRemoveCount] != ' ')
					CharacterRemoveCount--;
			}
			else
				CharacterRemoveCount = source.Length;
			res.Add((res.Count > 0 ? "     ":"") + source[..(source[CharacterRemoveCount-1]==' '? CharacterRemoveCount-1 : CharacterRemoveCount)]);
			source = source[CharacterRemoveCount..source.Length];
		}
		return res.ToArray();
	}

    public static string[] CompileTaskPreview(Task task) {
        string source = "";
        List<string> res = [];
        if (task.project != null)
            source += $"{task.project.Name}: ";
        if (task.ticket != null)
            source += $"{task.ticket.name}: ";
        source += task.description;
        while (source.Length > 0) {
            int CharacterRemoveCount;
            if (source.Length >= MAX_LINE_LENGTH) {
                CharacterRemoveCount = MAX_LINE_LENGTH;
                while (source[CharacterRemoveCount] != ' ')
                    CharacterRemoveCount--;
            } else
                CharacterRemoveCount = source.Length;
            res.Add((res.Count > 0 ? "     " : "") + source[..(source[CharacterRemoveCount - 1] == ' ' ? CharacterRemoveCount - 1 : CharacterRemoveCount)]);
            source = source[CharacterRemoveCount..source.Length];
        }
        return res.ToArray();
    }


    private void SetUtilityFields(DateTime selectedWeek) {
		BufferAnnotationValueUnsafe("week", Convert.ToString(dateTimeService.GetWeekCountAtDate(selectedWeek)));
		BufferFieldValueUnsafe("week", Convert.ToString(dateTimeService.GetWeekCountAtDate(selectedWeek)));
		BufferAnnotationValueUnsafe("name", settingsService.TryGetSetting(SettingsService.USER_NAME_KEY) ?? "username");
		BufferFieldValueUnsafe("name", settingsService.TryGetSetting(SettingsService.USER_NAME_KEY) ?? "username");
		BufferAnnotationValueUnsafe("job", settingsService.TryGetSetting(SettingsService.JOB_NAME_KEY) ?? "job name");
		BufferFieldValueUnsafe("job", settingsService.TryGetSetting(SettingsService.JOB_NAME_KEY) ?? "job name");
		DateTime dayFrom = DateTimeService.GetMondayOfWeekAtDate(selectedWeek);
		DateTime dayTo = dayFrom.AddDays(5);
		BufferAnnotationValueUnsafe("date_from", $"{dayFrom.Day}.{dayFrom.Month}. {dayFrom.Year}");
		BufferFieldValueUnsafe("date_from", $"{dayFrom.Day}.{dayFrom.Month}. {dayFrom.Year}");
		BufferAnnotationValueUnsafe("date_to", $"{dayTo.Day}.{dayTo.Month}. {dayTo.Year}");
		BufferFieldValueUnsafe("date_to", $"{dayTo.Day}.{dayTo.Month}. {dayTo.Year}");

        bool[] missingDays = new bool[5000];
        long startDateSeconds = DateTimeService.ToSeconds(settingsService.StartDate);
        long maxDateSeconds = DateTimeService.ToSeconds(DateTimeService.CeilWeek(cacheService.SelectedDay));
        List<Task> blockingTasks = _dbService.QueryBlockingTasksInIntervallAsync(startDateSeconds, maxDateSeconds).Result;
        foreach (Task t in blockingTasks) {
            long taskStartOffsetSeconds = t.start - startDateSeconds;
            int taskStartOffsetDays = (int)Math.Floor((double)taskStartOffsetSeconds / TimeSpan.SecondsPerDay);
            if (t.blocksTime == Database.BlockedTimeIntervallType.Sick | t.blocksTime == Database.BlockedTimeIntervallType.NoExcuse)
                missingDays[taskStartOffsetDays] = true;
        }
        startDateSeconds = DateTimeService.ToSeconds(DateTimeService.FloorWeek(cacheService.SelectedDay));
        bool[] newMissingDays = new bool[7];
        bool[] newSickDays = new bool[7];
        foreach (Task t in blockingTasks.Where(x => x.start > startDateSeconds).ToList()) {
            long taskStartOffsetSeconds = t.start - startDateSeconds;
            int taskStartOffsetDays = (int)Math.Floor((double)taskStartOffsetSeconds / TimeSpan.SecondsPerDay);
            if (t.blocksTime == Database.BlockedTimeIntervallType.NoExcuse)
                newMissingDays[taskStartOffsetDays] = true;
            if (t.blocksTime == Database.BlockedTimeIntervallType.Sick)
                newSickDays[taskStartOffsetDays] = true;
        }
		int totalMissingDaysCount = missingDays.Count(c => c == true);
		int missingDaysCount = newMissingDays.Count(c => c == true);
		int sickDaysCount = newSickDays.Count(c => c == true);
        BufferAnnotationValueUnsafe("total_sick_days", Convert.ToString(totalMissingDaysCount));
        BufferFieldValueUnsafe("total_sick_days", Convert.ToString(totalMissingDaysCount));
        BufferAnnotationValueUnsafe("new_missing_days", Convert.ToString(missingDaysCount));
        BufferFieldValueUnsafe("new_missing_days", Convert.ToString(missingDaysCount));
        BufferAnnotationValueUnsafe("new_sick_days", Convert.ToString(sickDaysCount));
        BufferFieldValueUnsafe("new_sick_days", Convert.ToString(sickDaysCount));
    }

	private string GetNewFileName(DateTime selectedWeek) {
		DateTime dayFrom = DateTimeService.GetMondayOfWeekAtDate(selectedWeek);
		DateTime dayTo = DateTimeService.GetFridayOfWeekAtDate(selectedWeek);
		string path = $"Ausbildungsnachweis{dateTimeService.GetWeekCountAtDate(selectedWeek)}_{dayFrom.Day}.{dayFrom.Month}. {dayFrom.Year}-{dayTo.Day}.{dayTo.Month}. {dayTo.Year}.pdf";
		Console.WriteLine($"generated file path:{path}");
		return path;
	}
}

public class PdfDocumentData {

	public const int DAY_LINE_COUNT = 6;
	public const int WEEK_LINE_COUNT = 5 * DAY_LINE_COUNT;
	public const int DOCUMENT_FIELD_COUNT = WEEK_LINE_COUNT + 9;

	public const int USER_NAME_INDEX = WEEK_LINE_COUNT;
	public const int JOB_NAME_INDEX = WEEK_LINE_COUNT + 1;
	public const int WEEK_INDEX = WEEK_LINE_COUNT + 2;
	public const int DATE_FOM_INDEX = WEEK_LINE_COUNT + 3;
	public const int DATE_TO_INDEX = WEEK_LINE_COUNT + 4;
	public const int SICK_DAYS_INDEX = WEEK_LINE_COUNT + 5;
	public const int MISSING_DAYS_INDEX = WEEK_LINE_COUNT + 6;
	public const int TOTAL_MISSING_DAYS_INDEX = WEEK_LINE_COUNT + 7;
	public const int TOTAL_TIME_INDEX = WEEK_LINE_COUNT + 8;

	public ValueTuple<string, string, string, Task>[] Data = new ValueTuple<string, string, string, Task>[DOCUMENT_FIELD_COUNT];

	public string UserName {
		set => Data[USER_NAME_INDEX].Item1 = value;
        get => Data[USER_NAME_INDEX].Item1;
    }
	public string JobName {
        set => Data[JOB_NAME_INDEX].Item1 = value;
        get => Data[JOB_NAME_INDEX].Item1;
    }
	public string Week {
        set => Data[WEEK_INDEX].Item1 = value;
        get => Data[WEEK_INDEX].Item1;
    }
	public string DateFrom {
        set => Data[DATE_FOM_INDEX].Item1 = value;
        get => Data[DATE_FOM_INDEX].Item1;
    }
	public string DateTo {
        set => Data[DATE_TO_INDEX].Item1 = value;
        get => Data[DATE_TO_INDEX].Item1;
    }
	public string SickDays {
        set => Data[SICK_DAYS_INDEX].Item1 = value;
        get => Data[SICK_DAYS_INDEX].Item1;
    }
	public string MissingDays {
        set => Data[MISSING_DAYS_INDEX].Item1 = value;
        get => Data[MISSING_DAYS_INDEX].Item1;
    }
	public string TotalMissingDays {
        set => Data[TOTAL_MISSING_DAYS_INDEX].Item1 = value;
        get => Data[TOTAL_MISSING_DAYS_INDEX].Item1;
    }
	public string TotalTime {
        set => Data[TOTAL_TIME_INDEX].Item1 = value;
        get => Data[TOTAL_TIME_INDEX].Item1;
    }

	public PdfDocumentData() {
        for (int i = 0; i < DOCUMENT_FIELD_COUNT; i++)
            Data[i] = new ValueTuple<string, string, string, Task>();
        JobName = "Example Job Name";
		UserName = "Example User";
		DateFrom = "1.10.1999";
		DateTo = "5.10.1999";
		Week = "11";
	}
}