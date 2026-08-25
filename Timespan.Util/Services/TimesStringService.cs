using System;

namespace Timespan.Util.Services;

public static partial class DateTimeService {

	#region HourMinute String

	public static string ToHourMinuteStringAbsolute(DateTime date) =>
		ToHourMinuteStringAbsolute(ToSeconds(date));

	public static string ToHourMinuteStringSinceMidnight(DateTime date) =>
		ToHourMinuteStringSinceMidnight(ToSeconds(date));

	public static string ToHourMinuteStringAbsolute(long totalSeconds) {
		long hours = totalSeconds / TimeSpan.SecondsPerHour;
		long minutes = (totalSeconds % TimeSpan.SecondsPerHour) / TimeSpan.SecondsPerMinute;
		return ToHourMinuteStringBase(minutes, hours);
	}

	public static string ToHourMinuteStringSinceMidnight(long totalSeconds) {
		long secondsSinceMidnight = totalSeconds % TimeSpan.SecondsPerDay;
		long hours = secondsSinceMidnight / TimeSpan.SecondsPerHour;
		long minutes = (secondsSinceMidnight % TimeSpan.SecondsPerHour) / TimeSpan.SecondsPerMinute;
		return ToHourMinuteStringBase(minutes, hours);
	}

	private static string ToHourMinuteStringBase(long minutes, long hours) {
		var hours_ = Convert.ToString(hours);
		var minutes_ = minutes < 10 ? "0" + Convert.ToString(minutes) : Convert.ToString(minutes);
		return hours_ + ":" + minutes_;
	}

	#endregion

	#region HourMinuteSeconds String

	public static string ToHourMinuteSecondsStringSinceMidnight(DateTime date) =>
		ToHourMinuteSecondsStringSinceMidnight(ToSeconds(date));

	public static string ToHourMinuteSecondsStringAbsolute(DateTime date) =>
		ToHourMinuteSecondsStringAbsolute(ToSeconds(date));

	public static string ToHourMinuteSecondsStringAbsolute(long totalSeconds) {
		long secondsSinceMidnight = totalSeconds % TimeSpan.SecondsPerDay;
		long hours = secondsSinceMidnight / TimeSpan.SecondsPerHour;
		long minutes = (secondsSinceMidnight % TimeSpan.SecondsPerHour) / TimeSpan.SecondsPerMinute;
		long seconds = (secondsSinceMidnight % TimeSpan.SecondsPerHour) % TimeSpan.SecondsPerMinute;
		return ToHourMinuteSecondStringBase(hours, minutes, seconds);
	}

	public static string ToHourMinuteSecondsStringSinceMidnight(long totalSeconds) {
		long secondsSinceMidnight = totalSeconds % TimeSpan.SecondsPerDay;
		long hours = secondsSinceMidnight / TimeSpan.SecondsPerHour;
		long minutes = (secondsSinceMidnight % TimeSpan.SecondsPerHour) / TimeSpan.SecondsPerMinute;
		long seconds = (secondsSinceMidnight % TimeSpan.SecondsPerHour) % TimeSpan.SecondsPerMinute;
		return ToHourMinuteSecondStringBase(hours, minutes, seconds);
	}

	private static string ToHourMinuteSecondStringBase(long hours, long minutes, long seconds) {
		var hours_ = Convert.ToString(hours);
		var minutes_ = minutes < 10 ? "0" + Convert.ToString(minutes) : Convert.ToString(minutes);
		var seconds_ = seconds < 10 ? "0" + Convert.ToString(seconds) : Convert.ToString(seconds);
		return hours_ + ":" + minutes_ + ":" + seconds_;
	}

	#endregion
}
