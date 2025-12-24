using ETMPClient.Enums;
using ETMPClient.Services;
using System;
using System.Windows.Input;

namespace ETMPClient.Commands
{
    public class NavigationCommand : CommandBase
    {
        private readonly INavigationService _navigationService;
        private readonly PageType _destinationPage;

        public NavigationCommand(INavigationService navigationService, PageType destinationPage)
        {
            _navigationService = navigationService;
            _destinationPage = destinationPage;
        }

        public override void Execute(object? parameter)
        {
            switch (_destinationPage)
            {
                case PageType.Home:
                    _navigationService.NavigateHome();
                    break;
                case PageType.Playlist:
                    _navigationService.NavigatePlaylist();
                    break;
                case PageType.Library:
                    _navigationService.NavigateLibrary();
                    break;
                case PageType.Midi:
                    _navigationService.NavigateMidi();
                    break;
                case PageType.Settings:
                    _navigationService.NavigateSettings();
                    break;
                case PageType.Equalizer:
                    _navigationService.NavigateEqualizer();
                    break;
            }
        }
    }
}
