using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace ETMPClient.Services
{
    public class LocalizationService
    {
        private static LocalizationService? _instance;
        public static LocalizationService Instance => _instance ??= new LocalizationService();

        private Dictionary<string, JsonElement> _translations = new();
        private string _currentLanguage = "en-US";

        public event EventHandler? LanguageChanged;

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    LoadLanguage(value);
                    LanguageChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public LocalizationService()
        {
            // Load default language
            LoadLanguage("en-US");
        }

        private void LoadLanguage(string languageCode)
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Languages", $"{languageCode}.json");
                
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var doc = JsonDocument.Parse(json);
                    _translations = new Dictionary<string, JsonElement>();
                    
                    foreach (var property in doc.RootElement.EnumerateObject())
                    {
                        _translations[property.Name] = property.Value.Clone();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load language {languageCode}: {ex.Message}");
            }
        }

        public string GetString(string key)
        {
            try
            {
                var keys = key.Split('.');
                JsonElement current = default;
                
                if (_translations.TryGetValue(keys[0], out current))
                {
                    for (int i = 1; i < keys.Length; i++)
                    {
                        if (current.TryGetProperty(keys[i], out var next))
                        {
                            current = next;
                        }
                        else
                        {
                            return key; // Return key if not found
                        }
                    }
                    
                    return current.GetString() ?? key;
                }
            }
            catch
            {
                // Return key if any error
            }
            
            return key;
        }

        public string this[string key] => GetString(key);
    }
}
