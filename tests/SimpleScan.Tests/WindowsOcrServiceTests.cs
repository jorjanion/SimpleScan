using SimpleScan.Core.Ocr;

namespace SimpleScan.Tests;

public class WindowsOcrServiceTests
{
    [Fact]
    public void OcrService_DetectsWindowsOcrAvailability()
    {
        var ocr = new WindowsOcrService();
        bool supported = ocr.IsOcrSupported();

        // On Windows 10/11 with language packs, OCR should be available
        Assert.True(supported);
        var langs = ocr.GetAvailableLanguages();
        Assert.NotEmpty(langs);
    }
}
