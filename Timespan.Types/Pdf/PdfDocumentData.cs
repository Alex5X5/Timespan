namespace Timespan.Types.Pdf;

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

	public PdfDocumentLine[] Data = new PdfDocumentLine[DOCUMENT_FIELD_COUNT];

	public string UserName {
		set => Data[USER_NAME_INDEX] = new(value, "", "", null);
		get => Data[USER_NAME_INDEX].Description;
	}
	public string JobName {
		set => Data[JOB_NAME_INDEX] = new(value, "", "", null);
		get => Data[JOB_NAME_INDEX].Description;
	}
	public string Week {
		set => Data[WEEK_INDEX] = new(value, "", "", null);
		get => Data[WEEK_INDEX].Description;
	}
	public string DateFrom {
		set => Data[DATE_FOM_INDEX] = new(value, "", "", null);
		get => Data[DATE_FOM_INDEX].Description;
	}
	public string DateTo {
		set => Data[DATE_TO_INDEX] = new(value, "", "", null);
		get => Data[DATE_TO_INDEX].Description;
	}
	public string SickDays {
		set => Data[SICK_DAYS_INDEX] = new(value, "", "", null);
		get => Data[SICK_DAYS_INDEX].Description;
	}
	public string MissingDays {
		set => Data[MISSING_DAYS_INDEX] = new(value, "", "", null);
		get => Data[MISSING_DAYS_INDEX].Description;
	}
	public string TotalMissingDays {
		set => Data[TOTAL_MISSING_DAYS_INDEX] = new(value, "", "", null);
		get => Data[TOTAL_MISSING_DAYS_INDEX].Description;
	}
	public string TotalTime {
		set => Data[TOTAL_TIME_INDEX] = new(value, "", "", null);
		get => Data[TOTAL_TIME_INDEX].Description		;
	}

	public PdfDocumentData() {
		for (int i = 0; i < DOCUMENT_FIELD_COUNT; i++)
			Data[i] = new PdfDocumentLine();
		UserName = "Example User";
		JobName = "Example Job Name";
		Week = "11";
		DateFrom = "1.10.1999";
		DateTo = "5.10.1999";
		SickDays = "1";
		MissingDays = "1";
		TotalMissingDays = "2";
		TotalTime = "0:00";
	}
}