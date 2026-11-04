using SimpleScan.Core.Models;

namespace SimpleScan.Core.Export;

public enum ExportImageFormat
{
    Png,
    Jpeg
}

public interface IImageExportService
{
    Task<IReadOnlyList<string>> ExportImagesAsync(
        IReadOnlyList<ScannedPage> pages,
        string targetDirectory,
        string baseFileName,
        ExportImageFormat format = ExportImageFormat.Jpeg,
        IProgress<string>? progress = null,
        CancellationToken ct = default);
}
