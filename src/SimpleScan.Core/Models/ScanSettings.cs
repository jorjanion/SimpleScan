using System.Text.Json.Serialization;

namespace SimpleScan.Core.Models;

public enum ScanColorMode
{
    Color = 1,
    Grayscale = 2,
    BlackAndWhite = 4
}

public class ScanSettings
{
    public string? SelectedDeviceId { get; set; }
    public int Dpi { get; set; } = 300;
    public ScanColorMode ColorMode { get; set; } = ScanColorMode.Color;
    public int Brightness { get; set; } = 0;
    public int Contrast { get; set; } = 0;
    public bool EnableOcr { get; set; } = true;
    public string OcrLanguage { get; set; } = "es-ES";
    public bool PromptSaveLocation { get; set; } = true;
    public string DefaultSaveDirectory { get; set; } = string.Empty;

    public ScanSettings()
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        DefaultSaveDirectory = Path.Combine(docs, "Escaneos");
    }
}
