namespace Timespan.PDF.Services.Interfaces;

using System;

using Types = Timespan.Types.Models;

public interface IPdfService {

    public void Export(IProgressReporter reporter, DateTime selectedWeek);
    public void Export(DateTime selectedWeek);
	public void Import();
	public Types.PdfDocumentData? GetExportData(DateTime selectedWeek);
}