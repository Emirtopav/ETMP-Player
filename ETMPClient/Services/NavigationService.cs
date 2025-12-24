using ETMPClient.Enums;
using ETMPClient.Events;
using ETMPClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETMPClient.Services
{
    public interface INavigationService
    {
        public event EventHandler<PageChangedEventArgs> PageChangedEvent;
        public PageType CurrentPage { get; }
        public void NavigateHome();
        public void NavigatePlaylist();
        public void NavigateLibrary();
        public void NavigateMidi();
        public void NavigateSettings();
        public void NavigateEqualizer();
    }

    public class NavigationService: INavigationService
    {
        private readonly Func<MainViewModel>? _mainViewModelFunc;
        private readonly Func<HomeViewModel>? _homeViewModelFunc;
        private readonly Func<PlaylistViewModel>? _playlistViewModelFunc;
        private readonly Func<LibraryViewModel>? _libraryViewModelFunc;
        private readonly Func<MidiViewModel>? _midiViewModelFunc;
        private readonly Func<SettingsViewModel>? _settingsViewModelFunc;
        private readonly Func<EqualizerViewModel>? _equalizerViewModelFunc;

        public event EventHandler<PageChangedEventArgs>? PageChangedEvent;
        public PageType CurrentPage { get; private set; } = PageType.Home;

        public NavigationService(Func<MainViewModel> mainViewModelFunc, Func<HomeViewModel> homeViewModelFunc,
                                 Func<PlaylistViewModel> playlistViewModelFunc,
                                 Func<LibraryViewModel> libraryViewModelFunc, Func<MidiViewModel> midiViewModelFunc,
                                 Func<SettingsViewModel> settingsViewModelFunc,
                                 Func<EqualizerViewModel> equalizerViewModelFunc)
        {
            _mainViewModelFunc = mainViewModelFunc;
            _homeViewModelFunc = homeViewModelFunc;
            _playlistViewModelFunc = playlistViewModelFunc;
            _libraryViewModelFunc = libraryViewModelFunc;
            _midiViewModelFunc = midiViewModelFunc;
            _settingsViewModelFunc = settingsViewModelFunc;
            _equalizerViewModelFunc = equalizerViewModelFunc;
        }

        public void NavigateHome()
        {
            var mainVm = _mainViewModelFunc?.Invoke();
            var homeVm = _homeViewModelFunc?.Invoke();

            if (mainVm != null && mainVm.CurrentView is not HomeViewModel)
            {
                mainVm.CurrentView = homeVm;
                CurrentPage = PageType.Home;
                PageChangedEvent?.Invoke(this, new PageChangedEventArgs(CurrentPage));
            }
        }

        public void NavigatePlaylist()
        {
            var mainVm = _mainViewModelFunc?.Invoke();
            var playlistVm = _playlistViewModelFunc?.Invoke();

            if (mainVm != null)
            {
                mainVm.CurrentView = playlistVm;
                CurrentPage = PageType.Playlist;
                PageChangedEvent?.Invoke(this, new PageChangedEventArgs(CurrentPage));
            }
        }

        public void NavigateLibrary()
        {
            var mainVm = _mainViewModelFunc?.Invoke();
            var libraryVm = _libraryViewModelFunc?.Invoke();

            if (mainVm != null && mainVm.CurrentView is not LibraryViewModel)
            {
                mainVm.CurrentView = libraryVm;
                CurrentPage = PageType.Library;
                PageChangedEvent?.Invoke(this, new PageChangedEventArgs(CurrentPage));
            }
        }

        public void NavigateMidi()
        {
            var mainVm = _mainViewModelFunc?.Invoke();
            var midiVm = _midiViewModelFunc?.Invoke();

            if (mainVm != null && mainVm.CurrentView is not MidiViewModel)
            {
                mainVm.CurrentView = midiVm;
                CurrentPage = PageType.Midi;
                PageChangedEvent?.Invoke(this, new PageChangedEventArgs(CurrentPage));
            }
        }

        public void NavigateSettings()
        {
            var mainVm = _mainViewModelFunc?.Invoke();
            var settingsVm = _settingsViewModelFunc?.Invoke();

            if (mainVm != null && mainVm.CurrentView is not SettingsViewModel)
            {
                mainVm.CurrentView = settingsVm;
                CurrentPage = PageType.Settings;
                PageChangedEvent?.Invoke(this, new PageChangedEventArgs(CurrentPage));
            }
        }


        public void NavigateEqualizer()
        {
            var mainVm = _mainViewModelFunc?.Invoke();
            var equalizerVm = _equalizerViewModelFunc?.Invoke();

            if (mainVm != null && mainVm.CurrentView is not EqualizerViewModel)
            {
                mainVm.CurrentView = equalizerVm;
                CurrentPage = PageType.Equalizer;
                PageChangedEvent?.Invoke(this, new PageChangedEventArgs(CurrentPage));
            }
        }
    }
}
