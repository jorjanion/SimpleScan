using SimpleScan.Core.Export;
using SimpleScan.Core.Models;

namespace SimpleScan.Tests;

public class ImageExportServiceTests
{
    [Fact]
    public async Task ExportImages_CreatesValidJpegFiles()
    {
        byte[] png1 = await TestImageGenerator.CreateSamplePngAsync(100, 100);
        byte[] png2 = await TestImageGenerator.CreateSamplePngAsync(100, 100);

        var page1 = new ScannedPage(png1, 1);
        var page2 = new ScannedPage(png2, 2);

        string tempDir = Path.Combine(Path.GetTempPath(), $"images_{Guid.NewGuid():N}");

        try
        {
            var exporter = new ImageExportService();
            var exported = await exporter.ExportImagesAsync(
                new[] { page1, page2 },
                tempDir,
                "Prueba",
                ExportImageFormat.Jpeg);

            Assert.Equal(2, exported.Count);
            foreach (var path in exported)
            {
                Assert.True(File.Exists(path));
                var bytes = await File.ReadAllBytesAsync(path);
                Assert.True(bytes.Length > 0);
            }
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, recursive: true);
        }
    }
}
