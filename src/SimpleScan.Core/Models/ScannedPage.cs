using System.IO;

namespace SimpleScan.Core.Models;

public class ScannedPage
{
    public Guid Id { get; } = Guid.NewGuid();
    public int PageNumber { get; set; }
    public byte[] RawImageBytes { get; private set; }
    public int Dpi { get; set; } = 300;
    public int PixelWidth { get; set; }
    public int PixelHeight { get; set; }
    public int RotationDegrees { get; private set; } = 0;
    public OcrResult? Ocr { get; set; }

    public ScannedPage(byte[] imageBytes, int pageNumber = 1, int dpi = 300, int width = 0, int height = 0)
    {
        RawImageBytes = imageBytes;
        PageNumber = pageNumber;
        Dpi = dpi;
        PixelWidth = width;
        PixelHeight = height;
    }

    public void RotateClockwise()
    {
        RotationDegrees = (RotationDegrees + 90) % 360;
    }

    public void RotateCounterClockwise()
    {
        RotationDegrees = (RotationDegrees + 270) % 360;
    }

    public void ResetRotation()
    {
        RotationDegrees = 0;
    }
}
