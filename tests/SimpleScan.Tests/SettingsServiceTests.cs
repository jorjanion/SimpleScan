using SimpleScan.Core.Configuration;
using SimpleScan.Core.Models;

namespace SimpleScan.Tests;

public class SettingsServiceTests
{
    [Fact]
    public void LoadSettings_DefaultValues_AreCorrect()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), $"settings_{Guid.NewGuid():N}.json");
        try
        {
            var service = new SettingsService(tempPath);
            var settings = service.LoadSettings();

            Assert.Equal(300, settings.Dpi);
            Assert.Equal(ScanColorMode.Color, settings.ColorMode);
            Assert.True(settings.EnableOcr);
            Assert.True(settings.PromptSaveLocation);
            Assert.NotEmpty(settings.DefaultSaveDirectory);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public void SaveSettings_PersistsModifiedValues()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), $"settings_{Guid.NewGuid():N}.json");
        try
        {
            var service = new SettingsService(tempPath);
            var settings = new ScanSettings
            {
                Dpi = 600,
                ColorMode = ScanColorMode.Grayscale,
                EnableOcr = false,
                PromptSaveLocation = false,
                SelectedDeviceId = "WIA-DEVICE-1234"
            };

            service.SaveSettings(settings);

            var loaded = service.LoadSettings();
            Assert.Equal(600, loaded.Dpi);
            Assert.Equal(ScanColorMode.Grayscale, loaded.ColorMode);
            Assert.False(loaded.EnableOcr);
            Assert.False(loaded.PromptSaveLocation);
            Assert.Equal("WIA-DEVICE-1234", loaded.SelectedDeviceId);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }
}
