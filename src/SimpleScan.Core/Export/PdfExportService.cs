using System.IO;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SimpleScan.Core.Models;
using SimpleScan.Core.Processing;

namespace SimpleScan.Core.Export;

public class PdfExportService : IPdfExportService
{
    public async Task ExportToPdfAsync(
        IReadOnlyList<ScannedPage> pages,
        string outputPath,
        bool embedOcrText = true,
        IProgress<string>? progress = null,
        CancellationToken ct = default)
    {
        if (pages == null || pages.Count == 0)
        {
            throw new ArgumentException("No hay páginas para exportar a PDF.", nameof(pages));
        }

        progress?.Report("Creando documento PDF...");

        string? dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var document = new PdfDocument();
        document.Info.Title = Path.GetFileNameWithoutExtension(outputPath);
        document.Info.Creator = "SimpleScan for Windows";

        int totalPages = pages.Count;
        for (int i = 0; i < totalPages; i++)
        {
            ct.ThrowIfCancellationRequested();
            var page = pages[i];
            progress?.Report($"Procesando página {i + 1} de {totalPages}...");

            byte[] processedBytes = await ImageProcessingHelper.RotateImageAsync(
                page.RawImageBytes,
                page.RotationDegrees,
                "png");

            var (pixelWidth, pixelHeight, dpiX, dpiY) = await ImageProcessingHelper.GetImageDimensionsAsync(processedBytes);
            if (dpiX <= 0) dpiX = page.Dpi > 0 ? page.Dpi : 300;
            if (dpiY <= 0) dpiY = page.Dpi > 0 ? page.Dpi : 300;

            // PDF dimensions in points (72 points per inch)
            double pointWidth = (pixelWidth / dpiX) * 72.0;
            double pointHeight = (pixelHeight / dpiY) * 72.0;

            if (pointWidth <= 0) pointWidth = 595; // A4 standard fallback
            if (pointHeight <= 0) pointHeight = 842;

            var pdfPage = document.AddPage();
            pdfPage.Width = new XUnit(pointWidth, XGraphicsUnit.Point);
            pdfPage.Height = new XUnit(pointHeight, XGraphicsUnit.Point);

            using (var gfx = XGraphics.FromPdfPage(pdfPage))
            {
                using var xImage = XImage.FromStream(() => new MemoryStream(processedBytes));

                gfx.DrawImage(xImage, 0, 0, pointWidth, pointHeight);

                // Embed invisible OCR text layer if enabled and available
                if (embedOcrText && page.Ocr != null && page.Ocr.HasText && page.RotationDegrees == 0)
                {
                    progress?.Report($"Incrustando capa de búsqueda OCR en página {i + 1}...");

                    double scaleX = pointWidth / pixelWidth;
                    double scaleY = pointHeight / pixelHeight;

                    var transparentBrush = new XSolidBrush(XColor.FromArgb(0, 0, 0, 0));

                    foreach (var line in page.Ocr.Lines)
                    {
                        foreach (var word in line.Words)
                        {
                            if (string.IsNullOrWhiteSpace(word.Text)) continue;

                            double x = word.BoundingRect.X * scaleX;
                            double y = word.BoundingRect.Y * scaleY;
                            double w = word.BoundingRect.Width * scaleX;
                            double h = word.BoundingRect.Height * scaleY;

                            double fontSize = Math.Max(4, h * 0.85);
                            try
                            {
                                var font = new XFont("Arial", fontSize);
                                gfx.DrawString(word.Text, font, transparentBrush, new XPoint(x, y + h * 0.8));
                            }
                            catch
                            {
                                // If font fails, continue with next word
                            }
                        }
                    }
                }
            }
        }

        ct.ThrowIfCancellationRequested();
        progress?.Report("Guardando archivo PDF...");
        document.Save(outputPath);
        progress?.Report("PDF guardado correctamente.");
    }
}
