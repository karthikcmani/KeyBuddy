using System;
using System.IO;
using System.Text.Json;

namespace KeyBuddy.Models;

public class AppSettings
{
    public bool AlwaysOnTop { get; set; } = true;
    public bool ShowNumpad { get; set; } = false;
    public bool ShowNavCluster { get; set; } = false;
    public bool CompactMode { get; set; } = false;
    public bool HighContrast { get; set; } = false;
    public bool DarkTheme { get; set; } = true;
    public double WindowOpacity { get; set; } = 1.0;
    public double KeySpacing { get; set; } = 4.0;
    public double KeyHeight { get; set; } = 48.0;
    public double WindowLeft { get; set; } = 200;
    public double WindowTop { get; set; } = 400;
    public double WindowWidth { get; set; } = 300;
    public double WindowHeight { get; set; } = 95;
    public bool LargeKeyMode { get; set; } = false;
    public bool StartWithWindows { get; set; } = true;
    public bool MiniTrioOnly { get; set; } = true;
    public bool EnableLongPressOForP { get; set; } = true;
    public bool EnableLongPress2For4 { get; set; } = true;
    public bool EnableLongPressAForAt { get; set; } = true;
    public int LongPressThresholdMs { get; set; } = 350;

    // List of keys to show in the Quick Rescue Bar (Default: P, 4, Backspace)
    public List<string> QuickBarKeys { get; set; } = new() { "P", "4", "Backspace" };

    // Dynamic Hold-to-Replace mapping rules
    public List<KeyMappingRule> CustomMappingRules { get; set; } = new()
    {
        new KeyMappingRule("O", "P", "Hold 'O' to type 'P'"),
        new KeyMappingRule("2", "4", "Hold '2' to type '4'"),
        new KeyMappingRule("A", "@", "Hold 'A' to type '@'")
    };

    private static readonly string SettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "KeyBuddy");
    private static readonly string SettingsFilePath = Path.Combine(SettingsFolder, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    return settings;
                }
            }
        }
        catch { }

        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            if (!Directory.Exists(SettingsFolder))
            {
                Directory.CreateDirectory(SettingsFolder);
            }
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }
        catch { }
    }
}
