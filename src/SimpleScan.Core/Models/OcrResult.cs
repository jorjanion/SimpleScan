namespace SimpleScan.Core.Models;

public record OcrRect(double X, double Y, double Width, double Height);

public record OcrWord(string Text, OcrRect BoundingRect);

public record OcrLine(string Text, IReadOnlyList<OcrWord> Words, OcrRect BoundingRect);

public record OcrResult(
    string FullText,
    IReadOnlyList<OcrLine> Lines,
    string LanguageTag
)
{
    public static OcrResult Empty(string languageTag = "es-ES") =>
        new(string.Empty, Array.Empty<OcrLine>(), languageTag);

    public bool HasText => !string.IsNullOrWhiteSpace(FullText);
}
