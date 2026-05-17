namespace Timespan.PDF.Services.Interfaces;

using System;

public interface IPdfService {

    public void Export(IProgressReporter reporter, DateTime selectedWeek);
    public void Export(DateTime selectedWeek);
	public void Import();
	public PdfDocumentData? GetExportData(DateTime selectedWeek);
}