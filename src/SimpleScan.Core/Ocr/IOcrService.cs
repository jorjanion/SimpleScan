using SimpleScan.Core.Models;

namespace SimpleScan.Core.Ocr;

public interface IOcrService
{
    bool IsOcrSupported();

    IReadOnlyList<string> GetAvailableLanguages();

    Task<OcrResult> RecognizeTextAsync(byte[] imageBytes, string? languageTag = null, CancellationToken ct = default);
}
