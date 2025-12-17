using System;
using System.IO;
using System.Text.Json;

namespace ETMPClient.Models
{
    public class EqualizerSettings
    {
        public float[] BandValues { get; set; } = new float[10];
        public float Preamp { get; set; } = 0;
        public string CurrentPreset { get; set; } = "Flat";
    }

    public static class EqualizerSettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ETMPPlayer",
            "equalizer_settings.json"
        );

        public static void Save(EqualizerSettings settings)
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save equalizer settings: {ex.Message}");
            }
        }

        public static EqualizerSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<EqualizerSettings>(json) ?? new EqualizerSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load equalizer settings: {ex.Message}");
            }

            return new EqualizerSettings();
        }
    }
}
