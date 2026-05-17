namespace Timespan.Types.Models;

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
