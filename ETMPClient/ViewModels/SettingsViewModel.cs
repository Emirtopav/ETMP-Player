using System;
using System.Windows.Input;
using ETMPClient.Commands;

namespace ETMPClient.ViewModels
{
    using ETMPClient.Services;

    public class SettingsViewModel : ViewModelBase
    {
        private readonly IMusicPlayerService _musicService;

        public float VisualizerSensitivity
        {
            get => _musicService.VisualizerSensitivity;
            set
            {
                if (_musicService.VisualizerSensitivity != value)
                {
                    _musicService.VisualizerSensitivity = value;
                    OnPropertyChanged();
                }
            }
        }

        public string VisualizerColorHex
        {
            get => _musicService.VisualizerColorHex;
            set
            {
                if (_musicService.VisualizerColorHex != value)
                {
                    _musicService.VisualizerColorHex = value;
                    OnPropertyChanged();
                }
            }
        }

        public double VisualizerOpacity
        {
            get => _musicService.VisualizerOpacity;
            set
            {
                if (_musicService.VisualizerOpacity != value)
                {
                    _musicService.VisualizerOpacity = value;
                    OnPropertyChanged();
                }
            }
        }

        public double VisualizerBrightness
        {
            get => _musicService.VisualizerBrightness;
            set
            {
                if (_musicService.VisualizerBrightness != value)
                {
                    _musicService.VisualizerBrightness = value;
                    OnPropertyChanged();
                }
            }
        }

        public double VisualizerBarThickness
        {
            get => _musicService.VisualizerBarThickness;
            set
            {
                if (_musicService.VisualizerBarThickness != value)
                {
                    _musicService.VisualizerBarThickness = value;
                    OnPropertyChanged();
                }
            }
        }

        public double VisualizerSpeed
        {
            get => _musicService.VisualizerSpeed;
            set
            {
                if (_musicService.VisualizerSpeed != value)
                {
                    _musicService.VisualizerSpeed = value;
                    OnPropertyChanged();
                }
            }
        }

        public string[] AvailableThemes => ThemeService.Instance.AvailableThemes;
        
        public string SelectedTheme
        {
            get => ThemeService.Instance.CurrentTheme;
            set
            {
                if (ThemeService.Instance.CurrentTheme != value)
                {
                    ThemeService.Instance.CurrentTheme = value;
                    OnPropertyChanged();
                }
            }
        }

        public double HomeTitleFontSize
        {
            get => _musicService.HomeTitleFontSize;
            set
            {
                if (_musicService.HomeTitleFontSize != value)
                {
                    _musicService.HomeTitleFontSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public double HomeArtistFontSize
        {
            get => _musicService.HomeArtistFontSize;
            set
            {
                if (_musicService.HomeArtistFontSize != value)
                {
                    _musicService.HomeArtistFontSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public double HomeCornerRadius
        {
            get => _musicService.HomeCornerRadius;
            set
            {
                if (_musicService.HomeCornerRadius != value)
                {
                    _musicService.HomeCornerRadius = value;
                    OnPropertyChanged();
                }
            }
        }

        public SettingsViewModel(IMusicPlayerService musicService)
        {
            _musicService = musicService;
        }
    }
}
