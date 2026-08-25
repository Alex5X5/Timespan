namespace Timespan.PDF.Services.Interfaces;

using System;

using Timespan.Types.Pdf;

public interface IPdfService {

    public void Export(DateTime selectedWeek);
	public void Import();
	public string GetFileNameForDate(DateTime selectedWeek);
	public PdfDocumentData? GetExportData(DateTime selectedWeek);
}