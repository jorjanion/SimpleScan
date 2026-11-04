using SimpleScan.Core.Models;

namespace SimpleScan.Core.Export;

public interface IPdfExportService
{
    Task ExportToPdfAsync(
        IReadOnlyList<ScannedPage> pages,
        string outputPath,
        bool embedOcrText = true,
        IProgress<string>? progress = null,
        CancellationToken ct = default);
}
