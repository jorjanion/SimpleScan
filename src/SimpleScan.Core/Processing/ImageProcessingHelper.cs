using System.IO;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace SimpleScan.Core.Processing;

public static class ImageProcessingHelper
{
    public static async Task<byte[]> RotateImageAsync(byte[] imageBytes, int rotationDegrees, string outputFormat = "png")
    {
        int normalizedRotation = ((rotationDegrees % 360) + 360) % 360;
        if (normalizedRotation == 0 && string.Equals(outputFormat, "png", StringComparison.OrdinalIgnoreCase))
        {
            return imageBytes;
        }

        using var inStream = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(inStream.GetOutputStreamAt(0)))
        {
            writer.WriteBytes(imageBytes);
            await writer.StoreAsync();
            await writer.FlushAsync();
        }

        var decoder = await BitmapDecoder.CreateAsync(inStream);

        var transform = new BitmapTransform();
        transform.Rotation = normalizedRotation switch
        {
            90 => BitmapRotation.Clockwise90Degrees,
            180 => BitmapRotation.Clockwise180Degrees,
            270 => BitmapRotation.Clockwise270Degrees,
            _ => BitmapRotation.None
        };

        var pixelData = await decoder.GetPixelDataAsync(
            decoder.BitmapPixelFormat,
            decoder.BitmapAlphaMode,
            transform,
            ExifOrientationMode.IgnoreExifOrientation,
            ColorManagementMode.ColorManageToSRgb);

        byte[] pixels = pixelData.DetachPixelData();

        uint rotatedWidth = (normalizedRotation == 90 || normalizedRotation == 270)
            ? decoder.PixelHeight
            : decoder.PixelWidth;

        uint rotatedHeight = (normalizedRotation == 90 || normalizedRotation == 270)
            ? decoder.PixelWidth
            : decoder.PixelHeight;

        using var outStream = new InMemoryRandomAccessStream();
        Guid encoderId = string.Equals(outputFormat, "jpg", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(outputFormat, "jpeg", StringComparison.OrdinalIgnoreCase)
            ? BitmapEncoder.JpegEncoderId
            : BitmapEncoder.PngEncoderId;

        var encoder = await BitmapEncoder.CreateAsync(encoderId, outStream);
        encoder.SetPixelData(
            decoder.BitmapPixelFormat,
            decoder.BitmapAlphaMode,
            rotatedWidth,
            rotatedHeight,
            decoder.DpiX,
            decoder.DpiY,
            pixels);

        await encoder.FlushAsync();

        var resultBytes = new byte[outStream.Size];
        using var reader = new DataReader(outStream.GetInputStreamAt(0));
        await reader.LoadAsync((uint)outStream.Size);
        reader.ReadBytes(resultBytes);

        return resultBytes;
    }

    public static async Task<(int width, int height, double dpiX, double dpiY)> GetImageDimensionsAsync(byte[] imageBytes)
    {
        try
        {
            using var stream = new InMemoryRandomAccessStream();
            using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
            {
                writer.WriteBytes(imageBytes);
                await writer.StoreAsync();
                await writer.FlushAsync();
            }

            var decoder = await BitmapDecoder.CreateAsync(stream);
            return ((int)decoder.PixelWidth, (int)decoder.PixelHeight, decoder.DpiX, decoder.DpiY);
        }
        catch
        {
            return (0, 0, 300, 300);
        }
    }
}
