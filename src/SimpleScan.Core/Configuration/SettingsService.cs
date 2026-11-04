using System.IO;
using System.Text.Json;
using SimpleScan.Core.Models;

namespace SimpleScan.Core.Configuration;

public class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string SettingsFilePath { get; }

    public SettingsService(string? customPath = null)
    {
        if (!string.IsNullOrEmpty(customPath))
        {
            SettingsFilePath = customPath;
        }
        else
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = Path.Combine(appData, "SimpleScan");
            SettingsFilePath = Path.Combine(folder, "settings.json");
        }
    }

    public ScanSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<ScanSettings>(json, JsonOptions);
                if (settings != null)
                {
                    return settings;
                }
            }
        }
        catch
        {
            // Fallback to defaults if file corrupt
        }

        return new ScanSettings();
    }

    public void SaveSettings(ScanSettings settings)
    {
        try
        {
            string? dir = Path.GetDirectoryName(SettingsFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = JsonSerializer.Serialize(settings, JsonOptions);
            string tempFile = SettingsFilePath + ".tmp";
            File.WriteAllText(tempFile, json);
            File.Move(tempFile, SettingsFilePath, overwrite: true);
        }
        catch
        {
            // Silently fail if settings can't be saved
        }
    }
}
