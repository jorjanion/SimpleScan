using System.IO;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace SimpleScan.Tests;

public static class TestImageGenerator
{
    public static async Task<byte[]> CreateSamplePngAsync(uint width = 200, uint height = 200, byte r = 255, byte g = 255, byte b = 255)
    {
        using var stream = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

        byte[] pixels = new byte[width * height * 4];
        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = b;     // Blue
            pixels[i + 1] = g; // Green
            pixels[i + 2] = r; // Red
            pixels[i + 3] = 255; // Alpha
        }

        encoder.SetPixelData(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            width,
            height,
            300,
            300,
            pixels);

        await encoder.FlushAsync();

        var bytes = new byte[stream.Size];
        using var reader = new DataReader(stream.GetInputStreamAt(0));
        await reader.LoadAsync((uint)stream.Size);
        reader.ReadBytes(bytes);

        return bytes;
    }
}
