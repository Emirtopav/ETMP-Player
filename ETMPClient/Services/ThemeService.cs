using System;
using System.Linq;
using System.Windows;

namespace ETMPClient.Services
{
    public class ThemeService
    {
        private static ThemeService? _instance;
        public static ThemeService Instance => _instance ??= new ThemeService();

        public string[] AvailableThemes { get; } = new[] { "Dark", "Light", "Midnight", "Winamp" };
        
        private string _currentTheme = "Dark";
        public string CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value && Array.Exists(AvailableThemes, t => t == value))
                {
                    _currentTheme = value;
                    ApplyTheme(value);
                }
            }
        }

        public event EventHandler? ThemeChanged;

        public void ApplyTheme(string themeName)
        {
            try
            {
                var app = Application.Current;
                if (app == null) return;

                // Remove old theme
                var oldTheme = app.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source?.OriginalString?.Contains("Theme.xaml") == true);
                if (oldTheme != null)
                {
                    app.Resources.MergedDictionaries.Remove(oldTheme);
                }

                // Add new theme
                string themeFile = themeName switch
                {
                    "Light" => "Themes/LightTheme.xaml",
                    "Midnight" => "Themes/MidnightTheme.xaml",
                    "Winamp" => "Themes/WinampTheme.xaml",
                    _ => "Themes/UIColors.xaml" // Dark (default)
                };

                var newTheme = new ResourceDictionary
                {
                    Source = new Uri(themeFile, UriKind.Relative)
                };

                app.Resources.MergedDictionaries.Insert(0, newTheme);
                
                ThemeChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply theme: {ex.Message}");
            }
        }
    }
}
