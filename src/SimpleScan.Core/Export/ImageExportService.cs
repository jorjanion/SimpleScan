using System.IO;
using SimpleScan.Core.Models;
using SimpleScan.Core.Processing;

namespace SimpleScan.Core.Export;

public class ImageExportService : IImageExportService
{
    public async Task<IReadOnlyList<string>> ExportImagesAsync(
        IReadOnlyList<ScannedPage> pages,
        string targetDirectory,
        string baseFileName,
        ExportImageFormat format = ExportImageFormat.Jpeg,
        IProgress<string>? progress = null,
        CancellationToken ct = default)
    {
        if (pages == null || pages.Count == 0)
        {
            throw new ArgumentException("No hay páginas para exportar.", nameof(pages));
        }

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        string ext = format == ExportImageFormat.Png ? "png" : "jpg";
        var outputPaths = new List<string>();

        int count = pages.Count;
        for (int i = 0; i < count; i++)
        {
            ct.ThrowIfCancellationRequested();
            progress?.Report($"Guardando imagen {i + 1} de {count}...");

            var page = pages[i];
            byte[] processedBytes = await ImageProcessingHelper.RotateImageAsync(
                page.RawImageBytes,
                page.RotationDegrees,
                ext);

            string fileName = count == 1
                ? $"{baseFileName}.{ext}"
                : $"{baseFileName}_{i + 1:D2}.{ext}";

            string fullPath = Path.Combine(targetDirectory, fileName);
            await File.WriteAllBytesAsync(fullPath, processedBytes, ct);
            outputPaths.Add(fullPath);
        }

        progress?.Report("Imágenes guardadas correctamente.");
        return outputPaths;
    }
}
