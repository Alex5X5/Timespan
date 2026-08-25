namespace Timespan.PDF.Services.Interfaces;

public interface IProgressReporter {
    
    void ReportProgress(int percentage, string? status = null);
    bool IsCancellationRequested { get; }

}
