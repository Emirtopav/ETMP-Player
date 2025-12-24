using System;
using System.Windows.Input;
using ETMPClient.Commands;

namespace ETMPClient.ViewModels
{
    using ETMPClient.Services;

    public class SettingsViewModel : ViewModelBase
    {
        private readonly IMusicPlayerService _musicService;

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
