namespace Timespan.Types.Pdf;

public record class PdfDocumentLine(
	string Description = "",
	string Hours = "",
	string HourRange = "",
	Models.Task? Task = null);