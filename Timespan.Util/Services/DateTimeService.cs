namespace Timespan.Util.Services;

using Timespan.Util.Services;
using System;

public class DateTimeService {

	//public static DateTime START_DATE = GetStartDate();

	private SettingsService settingsService;
	private CacheService cacheService;

	public DateTimeService(SettingsService settingsService, CacheService cacheService) {
		this.cacheService = cacheService;
		this.settingsService = settingsService;
	}

	public static int DayOfWorkWeek(DateTime day) =>
		(int)(day.DayOfWeek - 1 + 7) % 7;

	public static int WeeksInMonth(DateTime date) {
		int weeks = 0;
		int offset = DayOfWorkWeek(FloorMonth(date));
		if (offset != 0)
			weeks++;
		int totalDays = DateTime.DaysInMonth(date.Year, date.Month);
		totalDays -= offset;
		weeks += (int)Math.Ceiling(totalDays / 7.0);
		return weeks;
	}

    public static long ToSeconds(DateTime date) =>
		date.Ticks / TimeSpan.TicksPerSecond;


    public static DateTime FloorHour(DateTime date) =>
        new(date.Year, date.Month, date.Day, date.Hour, 0, 0);

    public static DateTime FloorDay(DateTime date) =>
		new(date.Year, date.Month, date.Day);

	public static DateTime FloorWeek(DateTime date) =>
		FloorDay(date.AddDays((-(int)date.DayOfWeek)+1));

	public static DateTime FloorMonth(DateTime date) =>
		new(date.Year, date.Month, 1);


	public static DateTime CeilHour(DateTime date) =>
		FloorHour(date).AddSeconds(3599);

	public static DateTime CeilDay(DateTime date) =>
		FloorDay(date).AddSeconds(TimeSpan.SecondsPerDay - 1);

	public static DateTime CeilWeek(DateTime date) =>
		FloorWeek(date).AddSeconds(TimeSpan.SecondsPerDay * 5 - 1);

	public static DateTime CeilMonth(DateTime date) =>
		FloorMonth(date).AddMonths(1).AddSeconds(-1);


    public static int DaysInCurrentMonth() =>
		DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

	public static bool TodayIsDayOfWeek(DayOfWeek day) =>
		day == DateTime.Now.DayOfWeek;

	public static bool TodayIsDayOfWeek(int day) =>
		day == (int)DateTime.Now.AddDays(-1).DayOfWeek;


	public static DateTime? InterpretDayAndTimeString(string s, int day = 1, int month = 1, int hour = 0, int minute = 0, int second = 0) {
		if (s == "")
			return null;
		int startIndex = 0;
		int finishIndex = 0;
		int getSelectionValue(char? nextSeperator, int nextOffset, int defaultValue) {
			int result = defaultValue;
			try {
				if (nextSeperator != null)
					finishIndex = s.IndexOf(nextSeperator ?? ' ', startIndex);
				else
					finishIndex = s.Length;
				if (finishIndex == -1)
					return result;
				result = Convert.ToInt32(s.Substring(startIndex, finishIndex - startIndex));
                startIndex = finishIndex + nextOffset;
            } catch (FormatException) {
                return result;
            }
            return result;
        }
		DateTime res = new(DateTime.Now.Year, month, day, hour, minute, second);
        try {
            res = new DateTime(res.Year, res.Month, getSelectionValue('.', 1, day));
        } catch (ArgumentOutOfRangeException) {
            return res;
        }
		try {
            res = new DateTime(res.Year, getSelectionValue('.', 2, month), res.Day);
		} catch (ArgumentOutOfRangeException) {
			return res;
		}
        try {
            res = new DateTime(res.Year, res.Month, res.Day, getSelectionValue(':', 1, hour), 0, 0);
        } catch (ArgumentOutOfRangeException) {
            return res;
        }
        try {
            res = new DateTime(res.Year, res.Month, res.Day, res.Hour, getSelectionValue(':', 1, minute), 0);
        } catch (ArgumentOutOfRangeException) {
            return res;
        }
        try {
            res = new DateTime(res.Year, res.Month, res.Day, res.Hour, res.Minute, getSelectionValue(null, 0, second));
        } catch (ArgumentOutOfRangeException) {
            return res;
        }
        return res;
	}

    public static DateTime? InterpretDayAndMonthAndYearString(string s) {
        try {
            int startIndex = 0;
            int finishIndex = s.IndexOf('.', startIndex);
            int day = Convert.ToInt32(s.Substring(startIndex, finishIndex - startIndex));
            startIndex = finishIndex + 1;
            finishIndex = s.IndexOf('.', startIndex);
            int month = Convert.ToInt32(s.Substring(startIndex, finishIndex - startIndex));
            startIndex = finishIndex + 1;
			finishIndex = s.Length;
            int year = Convert.ToInt32(s.Substring(startIndex, finishIndex - startIndex));
            DateTime time = new(year, month, day);
            return time;
        } catch (FormatException) {
            return null;
        } catch (ArgumentOutOfRangeException) {
            return null;
        }
    }


    public static string ToTimeString(DateTime time) =>
		$"{time.Hour}:{time.Minute}:{time.Second}";

	public static string ToHourMinuteString(long totalSeconds) {
		long hours = (totalSeconds % TimeSpan.SecondsPerDay) / TimeSpan.SecondsPerHour;
		long minutes = ((totalSeconds % TimeSpan.SecondsPerDay) % TimeSpan.SecondsPerHour) / TimeSpan.SecondsPerMinute;
		return (hours < 10 ? "0" + Convert.ToString(hours) : Convert.ToString(hours)) + ":" + (minutes < 10 ? "0" + Convert.ToString(minutes) : Convert.ToString(minutes));
	}

	public static string ToDayAndMonthAndTimeString(DateTime time) =>
		$"{time.Day}.{time.Month}. {time.Hour}:{time.Minute}:{time.Second}";

	public static string ToDayAndMonthAndYearString(DateTime time) =>
		$"{time.Day}.{time.Month}.{time.Year}";

	public static string ToDayAndMonthString(DateTime time) =>
		$"{time.Day}.{time.Month}.";


	public static DateTime GetMondayOfCurrentWeek() {
        return FloorWeek(DateTime.Now);
    }

	public static DateTime GetMondayOfWeekAtDate(DateTime date) =>
		FloorWeek(date);

	public static DateTime GetFridayOfCurrentWeek() =>
		GetMondayOfCurrentWeek().AddDays(4);
	
	public static DateTime GetFridayOfWeekAtDate(DateTime date) =>
		GetMondayOfWeekAtDate(date).AddDays(4);

	public static DateTime GetFirstDayOfCurrentMonth() =>
		new(DateTime.Today.Year, DateTime.Today.Month, 1);

	public static DateTime GetFirstDayOfMonthAtDate(DateTime date) =>
		new(date.Year, date.Month, 1);


	public int GetCurrentWeekCount() =>
		GetWeekCountAtDate(DateTime.Now);

	public int GetWeekCountAtDate(DateTime date) =>
		(int)Math.Floor(FloorWeek(date).Subtract(settingsService.StartDate).Days / 7.0) + 1;
}
