namespace Timespan.PDF.Services.Interfaces;

using System;

using Timespan.Types.Pdf;

using Types = Timespan.Types.Models;

public interface IPdfService {

    public void Export(DateTime selectedWeek);
	public void Import();
	public PdfDocumentData? GetExportData(DateTime selectedWeek);
}