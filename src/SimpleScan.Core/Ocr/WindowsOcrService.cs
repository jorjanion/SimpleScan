using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using ModelOcrResult = SimpleScan.Core.Models.OcrResult;
using ModelOcrLine = SimpleScan.Core.Models.OcrLine;
using ModelOcrWord = SimpleScan.Core.Models.OcrWord;
using ModelOcrRect = SimpleScan.Core.Models.OcrRect;

namespace SimpleScan.Core.Ocr;

public class WindowsOcrService : IOcrService
{
    public bool IsOcrSupported()
    {
        try
        {
            return OcrEngine.AvailableRecognizerLanguages.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    public IReadOnlyList<string> GetAvailableLanguages()
    {
        try
        {
            return OcrEngine.AvailableRecognizerLanguages
                .Select(l => l.LanguageTag)
                .ToList();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public async Task<ModelOcrResult> RecognizeTextAsync(byte[] imageBytes, string? languageTag = null, CancellationToken ct = default)
    {
        if (imageBytes == null || imageBytes.Length == 0)
        {
            return ModelOcrResult.Empty(languageTag ?? "es-ES");
        }

        OcrEngine? engine = null;
        if (!string.IsNullOrWhiteSpace(languageTag))
        {
            try
            {
                var lang = new Language(languageTag);
                if (OcrEngine.IsLanguageSupported(lang))
                {
                    engine = OcrEngine.TryCreateFromLanguage(lang);
                }
            }
            catch
            {
                // Fallback to user language
            }
        }

        // Try Spanish if requested language wasn't available
        if (engine == null)
        {
            try
            {
                var es = new Language("es-ES");
                if (OcrEngine.IsLanguageSupported(es))
                {
                    engine = OcrEngine.TryCreateFromLanguage(es);
                }
            }
            catch { }
        }

        // Fallback to profile languages
        engine ??= OcrEngine.TryCreateFromUserProfileLanguages();

        if (engine == null)
        {
            return ModelOcrResult.Empty(languageTag ?? "es-ES");
        }

        ct.ThrowIfCancellationRequested();

        using var randomAccessStream = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(randomAccessStream.GetOutputStreamAt(0)))
        {
            writer.WriteBytes(imageBytes);
            await writer.StoreAsync();
            await writer.FlushAsync();
        }

        ct.ThrowIfCancellationRequested();

        var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
        var softwareBitmap = await decoder.GetSoftwareBitmapAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied);

        ct.ThrowIfCancellationRequested();

        var winOcrResult = await engine.RecognizeAsync(softwareBitmap);

        var lines = new List<ModelOcrLine>();
        foreach (var line in winOcrResult.Lines)
        {
            var words = new List<ModelOcrWord>();
            foreach (var word in line.Words)
            {
                var r = word.BoundingRect;
                words.Add(new ModelOcrWord(word.Text, new ModelOcrRect(r.X, r.Y, r.Width, r.Height)));
            }

            var lineRect = words.Count > 0
                ? new ModelOcrRect(
                    words.Min(w => w.BoundingRect.X),
                    words.Min(w => w.BoundingRect.Y),
                    words.Max(w => w.BoundingRect.X + w.BoundingRect.Width) - words.Min(w => w.BoundingRect.X),
                    words.Max(w => w.BoundingRect.Y + w.BoundingRect.Height) - words.Min(w => w.BoundingRect.Y))
                : new ModelOcrRect(0, 0, 0, 0);

            lines.Add(new ModelOcrLine(line.Text, words, lineRect));
        }

        return new ModelOcrResult(
            winOcrResult.Text,
            lines,
            engine.RecognizerLanguage.LanguageTag);
    }
}
