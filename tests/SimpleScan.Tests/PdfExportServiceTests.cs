using SimpleScan.Core.Export;
using SimpleScan.Core.Models;

namespace SimpleScan.Tests;

public class PdfExportServiceTests
{
    [Fact]
    public async Task ExportToPdf_CreatesValidPdfFile()
    {
        byte[] pngBytes = await TestImageGenerator.CreateSamplePngAsync(300, 400);

        var ocr = new OcrResult(
            "Documento escaneado de prueba",
            new List<OcrLine>
            {
                new("Documento escaneado de prueba", new List<OcrWord>
                {
                    new("Documento", new OcrRect(10, 10, 80, 20)),
                    new("escaneado", new OcrRect(95, 10, 80, 20)),
                    new("de", new OcrRect(180, 10, 20, 20)),
                    new("prueba", new OcrRect(205, 10, 60, 20))
                }, new OcrRect(10, 10, 255, 20))
            },
            "es-ES");

        var page = new ScannedPage(pngBytes, 1, 300, 300, 400)
        {
            Ocr = ocr
        };

        string tempPdf = Path.Combine(Path.GetTempPath(), $"test_scan_{Guid.NewGuid():N}.pdf");

        try
        {
            var exporter = new PdfExportService();
            await exporter.ExportToPdfAsync(new[] { page }, tempPdf, embedOcrText: true);

            Assert.True(File.Exists(tempPdf));
            var fileBytes = await File.ReadAllBytesAsync(tempPdf);
            Assert.True(fileBytes.Length > 100);

            // Check standard PDF magic header (%PDF-)
            string header = System.Text.Encoding.ASCII.GetString(fileBytes, 0, 5);
            Assert.Equal("%PDF-", header);
        }
        finally
        {
            if (File.Exists(tempPdf)) File.Delete(tempPdf);
        }
    }
}
