namespace Timespan.PDF.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Timespan.Database.Services.Interfaces;
using Timespan.PDF.Services.Interfaces;
using Timespan.Types.Pdf;
using Timespan.Util.Services;

using Types = Timespan.Types.Models;

public unsafe partial class PdfService : IPdfService, IDisposable {

	private readonly ITimespanDbService _dbService;
	private SettingsService settingsService;
	CacheService cacheService;

	public const int MAX_LINE_LENGTH = 85;

	private const string LAST_SECTION_INDEXER = "eof";

	public static bool IndexersLoaded { private set; get; } = false;

	private readonly Dictionary<string, ValueTuple<IntPtr, IntPtr>> Indexers;

	public ConcurrentDictionary<string, string> InsertOperations;

	private readonly int charCount;
	private readonly char* text;

	public PdfService(ITimespanDbService dbService, SettingsService settingsService, CacheService cacheService) {
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
			counter++;
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
		List<Types.Task> tasks = _dbService.QueryTasksOfWeekAtDateAsync(selectedWeek).Result;
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
		foreach(var dayName in days.Keys) {
			int offset = 0;
			string[] lines = ["", "", "", "", "", ""];
			List<Types.Task> tasks_ = tasks.Where(x => x.StartDateTime.DayOfWeek == days[dayName]).ToList();
			if (tasks_.Count == 0)
				continue;
			foreach (Types.Task task in tasks_) {
				if (task.running)
					continue;
				string[] compiledTask = CompileTask(task);
				try {
					Array.ConstrainedCopy(compiledTask, 0, lines, offset, compiledTask.Length);
					query = $"{dayName}_hour_range_{offset + 1}";
					value = DateTimeService.ToHourMinuteStringSinceMidnight(task.start) + " - " + DateTimeService.ToHourMinuteStringSinceMidnight(task.finish);
					BufferValue(query, value);
					query = $"{dayName}_hour_{offset + 1}";
					value = DateTimeService.ToHourMinuteStringSinceMidnight(task.finish - task.start);
					BufferValue(query, value);
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
			for (int i = 0; i < lines.Length; i++) {
				query = $"{dayName}_line_{i + 1}";
				BufferValue(query, lines[i]);
			}
		}
		//foreach (string dayName in days.Keys) {
		//}
		query = $"total_hour";
		value = DateTimeService.ToHourMinuteStringSinceMidnight(totalWeekSeconds);
		BufferValue(query, value);
		SetUtilityFields(selectedWeek);
		prepareContentStopwatch.Stop();
		Console.Write("finished preparing content for the document\n");
		Console.WriteLine($"preparing content took {prepareContentStopwatch.ElapsedMilliseconds / 1000.0} seconds");
		char* document = BuildDocument(out int documentCharCount);
		byte* resultFile = FileService.EncodeBufferAnsi(document, documentCharCount, out int fileSize);
		FileService.WriteFileUnsafe(resultFile, PathService.FilesPath($"Nachweise/{GetFileNameForDate(selectedWeek)}"), fileSize);
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
		List<Types.Task> tasks = _dbService.QueryAllTasksOfWeekAtDateAsync(selectedWeek).Result;
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
			Types.Task[] lineTaks = new Types.Task[lines.Length];
			string[] hours = ["", "", "", "", "", ""];
			string[] hourRanges = ["", "", "", "", "", ""];
			List<Types.Task> tasks_ = tasks.Where(x => x.StartDateTime.DayOfWeek == days[dayName]).ToList();
            if (tasks_.Count == 0) {
				dayCounter++;
				continue;
			}
			DateTime date_ = DateTimeService.FloorDay(DateTimeService.FloorWeek(selectedWeek).AddDays(dayCounter));
            Types.Task? blockedBy = _dbService.QueryBlockingTasksAtDateAsync(date_).Result.FirstOrDefault();
			if (blockedBy != null) {
				lines[0] = blockedBy.description;
			} else {
				foreach (Types.Task task in tasks_) {
					if (task.running)
						continue;
					string[] compiledTask = CompileTaskPreview(task);
					try {
						Array.ConstrainedCopy(compiledTask, 0, lines, offset, compiledTask.Length);
						for (int i = 0; i < compiledTask.Length; i++)
							lineTaks[offset + i] = task;
						for (int i = 0; i < compiledTask.Length; i++)
							lineTaks[offset + i] = task;
						hourRanges[offset] = DateTimeService.ToHourMinuteStringSinceMidnight(task.start) + " - " + DateTimeService.ToHourMinuteStringSinceMidnight(task.finish);
						hours[offset] = DateTimeService.ToHourMinuteStringSinceMidnight(task.finish - task.start);
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
				data.Data[dayCounter * PdfDocumentData.DAY_LINE_COUNT + i] = new(lines[i], hours[i], hourRanges[i], lineTaks[i]);
            }
			dayCounter++;
		}
		data.TotalTime = DateTimeService.ToHourMinuteStringAbsolute(totalWeekSeconds);
		int week = DateTimeService.GetWeekCountAtDate(settingsService.StartDate, selectedWeek);
        data.Week = Convert.ToString(week);
		data.UserName = settingsService.TryGetSetting(SettingsService.USER_NAME_KEY) ?? "username";
		data.JobName = settingsService.TryGetSetting(SettingsService.JOB_NAME_KEY) ?? "job name";
		bool[] missingDays = new bool[5000];
		long startDateSeconds = DateTimeService.ToSeconds(settingsService.StartDate);
		long maxDateSeconds = DateTimeService.ToSeconds(DateTimeService.CeilWeek(cacheService.SelectedDay));
		List<Types.Task> blockingTasks = _dbService.QueryBlockingTasksInIntervallAsync(startDateSeconds, maxDateSeconds).Result;
        foreach (Types.Task t in blockingTasks) {
			long taskStartOffsetSeconds = t.start - startDateSeconds;
			int taskStartOffsetDays = (int)Math.Floor((double)taskStartOffsetSeconds / TimeSpan.SecondsPerDay);
			if(t.blocksTime==Types.BlockedTimeIntervallType.Sick | t.blocksTime == Types.BlockedTimeIntervallType.NoExcuse)
				missingDays[taskStartOffsetDays] = true;
        }
		data.TotalMissingDays=Convert.ToString(missingDays.Count(c => c ==true));
        startDateSeconds = DateTimeService.ToSeconds(DateTimeService.FloorWeek(cacheService.SelectedDay));
        bool[] newMissingDays = new bool[7];
        bool[] newSickDays = new bool[7];
        foreach (Types.Task t in blockingTasks.Where(x=>x.start>startDateSeconds).ToList()) {
            long taskStartOffsetSeconds = t.start - startDateSeconds;
            int taskStartOffsetDays = (int)Math.Floor((double)taskStartOffsetSeconds / TimeSpan.SecondsPerDay);
            if (t.blocksTime == Types.BlockedTimeIntervallType.NoExcuse)
                newMissingDays[taskStartOffsetDays] = true;
            if (t.blocksTime == Types.BlockedTimeIntervallType.Sick)
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

	public static string[] CompileTask(Types.Task task) {
		var description = task.description;
		description = description.Replace("""\""", """\\""");
		description = description.Replace("""(""", """\(""");
		description = description.Replace(""")""", """\)""");
		return CompileTaskBase(description);
	}

    public static string[] CompileTaskPreview(Types.Task task) {
		return CompileTaskBase(task.description);
    }

	private static string[] CompileTaskBase(string description) {
		string source = description;
		List<string> res = [];
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
		var week = DateTimeService.GetWeekCountAtDate(settingsService.StartDate, selectedWeek);
		BufferValue("week", Convert.ToString(week));
		BufferValue("name", settingsService.TryGetSetting(SettingsService.USER_NAME_KEY) ?? "username");
		BufferValue("job", settingsService.TryGetSetting(SettingsService.JOB_NAME_KEY) ?? "job name");
		DateTime dayFrom = DateTimeService.FloorWeek(selectedWeek);
		DateTime dayTo = dayFrom.AddDays(5);
		BufferValue("date_from", $"{dayFrom.Day}.{dayFrom.Month}. {dayFrom.Year}");
		BufferValue("date_to", $"{dayTo.Day}.{dayTo.Month}. {dayTo.Year}");

        bool[] missingDays = new bool[5000];
        long startDateSeconds = DateTimeService.ToSeconds(settingsService.StartDate);
        long maxDateSeconds = DateTimeService.ToSeconds(DateTimeService.CeilWeek(cacheService.SelectedDay));
        List<Types.Task> blockingTasks = _dbService.QueryBlockingTasksInIntervallAsync(startDateSeconds, maxDateSeconds).Result;
        foreach (Types.Task t in blockingTasks) {
            long taskStartOffsetSeconds = t.start - startDateSeconds;
            int taskStartOffsetDays = (int)Math.Floor((double)taskStartOffsetSeconds / TimeSpan.SecondsPerDay);
            if (t.blocksTime == Types.BlockedTimeIntervallType.Sick | t.blocksTime == Types.BlockedTimeIntervallType.NoExcuse)
                missingDays[taskStartOffsetDays] = true;
        }
        startDateSeconds = DateTimeService.ToSeconds(DateTimeService.FloorWeek(cacheService.SelectedDay));
        bool[] newMissingDays = new bool[7];
        bool[] newSickDays = new bool[7];
        foreach (Types.Task t in blockingTasks.Where(x => x.start > startDateSeconds).ToList()) {
            long taskStartOffsetSeconds = t.start - startDateSeconds;
            int taskStartOffsetDays = (int)Math.Floor((double)taskStartOffsetSeconds / TimeSpan.SecondsPerDay);
            if (t.blocksTime == Types.BlockedTimeIntervallType.NoExcuse)
                newMissingDays[taskStartOffsetDays] = true;
            if (t.blocksTime == Types.BlockedTimeIntervallType.Sick)
                newSickDays[taskStartOffsetDays] = true;
        }
		int totalMissingDaysCount = missingDays.Count(c => c == true);
		int missingDaysCount = newMissingDays.Count(c => c == true);
		int sickDaysCount = newSickDays.Count(c => c == true);
        BufferValue("total_sick_days", Convert.ToString(totalMissingDaysCount));
        BufferValue("new_missing_days", Convert.ToString(missingDaysCount));
        BufferValue("new_sick_days", Convert.ToString(sickDaysCount));
    }

	public string GetFileNameForDate(DateTime selectedWeek) {
		DateTime dayFrom = DateTimeService.GetMondayOfWeekAtDate(selectedWeek);
		DateTime dayTo = DateTimeService.GetFridayOfWeekAtDate(selectedWeek);
		var week = DateTimeService.GetWeekCountAtDate(settingsService.StartDate, selectedWeek);
		string path = $"Ausbildungsnachweis{week}_{dayFrom.Day}.{dayFrom.Month}. {dayFrom.Year}-{dayTo.Day}.{dayTo.Month}. {dayTo.Year}.pdf";
		Console.WriteLine($"generated file path:{path}");
		return path;
	}
}