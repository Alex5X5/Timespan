namespace Timespan.Util.Services;

using System;

public partial class SettingsService {

	public string Username {
        set {
            SetSetting(USER_NAME_KEY, value);
            OnUsernameChanged?.Invoke(nameof(Username));
        }
        get => GetSetting(USER_NAME_KEY);
    }
	public event Action<string>? OnUsernameChanged;

	private DateTime startDate = DateTime.MinValue;
	
	public DateTime StartDate {
		set => StartDateString = DateTimeService.ToDayAndMonthAndYearString(value);
		get => DateTimeService.InterpretDayAndMonthAndYearString(StartDateString) ?? DateTime.MinValue;
	}

	public string StartDateString {
        set {
			SetSetting(START_DATE_KEY, value);
            OnStartDateChanged?.Invoke(StartDateString);
        }
        get => GetSetting(START_DATE_KEY);

    }

	public event Action<string>? OnStartDateChanged;

	public string JobName {
		set {
			SetSetting(JOB_NAME_KEY, value);
			OnJobNameChanged?.Invoke(JobName);
		}
		get => GetSetting(JOB_NAME_KEY);
	}

	public event Action<string>? OnJobNameChanged;
}
