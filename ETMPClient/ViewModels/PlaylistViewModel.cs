using ETMPClient.Commands;
using ETMPClient.Core;
using ETMPClient.Enums;
using ETMPClient.Events;
using ETMPClient.Extensions;
using ETMPClient.Interfaces;
using ETMPClient.Models;
using ETMPClient.Services;
using ETMPClient.Stores;
using ETMPData.DataEntities;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace ETMPClient.ViewModels
{
    public class PlaylistViewModel : ViewModelBase, IFilesDropAsync
    {
        private readonly IMusicPlayerService _musicService;
        private readonly PlaylistBrowserNavigationStore _playlistBrowserNavigationStore;
        private readonly MediaStore _mediaStore;
        private readonly PlaylistStore _playlistStore;
        public string CurrentDateString { get; }

        public string? _currentPlaylistName;
        public string? _bannerUrl;
        public string? CurrentPlaylistName
        {
            get => _currentPlaylistName;
            set
            {
                _currentPlaylistName = value;
                OnPropertyChanged();
            }
        }

        public string? BannerUrl 
        { 
            get { return _bannerUrl; }
            set
            {
                if(_bannerUrl != value)
                {
                    _bannerUrl = value;
                    OnPropertyChanged(nameof(BannerUrl));
                }
            }
        }

        public string PlaylistCreationDate { get; }

        public ObservableCollection<MediaModel>? AllSongsOfPlaylist { get; set; }
        public ICommand? RenamePlaylist { get; }
        public ICommand? PlaySong { get; }
        public ICommand? OpenExplorer { get; }
        public ICommand? AddSong { get; set; }
        public ICommand? ChangeBanner { get; set; }
        public ICommand? DeleteSong { get; set; }

        public PlaylistViewModel(IMusicPlayerService musicService, INavigationService navigationService, MediaStore mediaStore, PlaylistStore playlistStore, PlaylistBrowserNavigationStore playlistBrowserNavigationStore)
        {
            _musicService = musicService;

            _playlistBrowserNavigationStore = playlistBrowserNavigationStore;

            _mediaStore = mediaStore;
            _playlistStore = playlistStore;

            RenamePlaylist = new RenamePlaylistAsyncCommand(_playlistStore, _playlistBrowserNavigationStore);
            ChangeBanner = new ChangeBannerAsyncCommand(_playlistStore, _playlistBrowserNavigationStore);

            _musicService.MusicPlayerEvent += OnMusicPlayerEvent;
            _mediaStore.PlaylistSongsAdded += OnPlaylistSongsAdded;
            _playlistStore.PlaylistBannerChanged += OnPlaylistBannerChange;

            PlaySong = new PlaySpecificSongCommand(musicService);

            OpenExplorer = new OpenExplorerAtPathCommand();

            CurrentPlaylistName = playlistStore.Playlists.FirstOrDefault(x => x.Id == playlistBrowserNavigationStore.BrowserPlaylistId)?.Name ?? "Undefined";

            CurrentDateString = DateTime.Now.ToString("dd MMM, yyyy");

            BannerUrl = playlistStore.Playlists.FirstOrDefault(x => x.Id == playlistBrowserNavigationStore.BrowserPlaylistId)?.Banner;
            
            PlaylistCreationDate = playlistStore.Playlists.FirstOrDefault(x => x.Id == playlistBrowserNavigationStore.BrowserPlaylistId)?.CreationDate?.ToString("dd MMM, yyyy") ?? DateTime.Now.ToString("dd MMM, yyyy");

            Task.Run(LoadSongs);
        }

        // TODO: Fix number hierarchy after DeleteSong called
        private void LoadSongs()
        {
            AllSongsOfPlaylist = new ObservableCollection<MediaModel>(_mediaStore.Songs.Where(x => x.PlayerlistId == _playlistBrowserNavigationStore.BrowserPlaylistId).Select((x, num) =>
            {
                return new MediaModel
                {
                    Playing = _musicService.PlayerState == PlaybackState.Playing && x.Id == _musicService.CurrentMedia?.Id,
                    Number = num + 1,
                    Id = x.Id,
                    Title = Path.GetFileNameWithoutExtension(x.FilePath),
                    Path = x.FilePath,
                    Duration = AudioUtills.DurationParse(x.FilePath)
                };
            }).ToList());
            DeleteSong = new DeleteSpecificSongAsyncCommand(_musicService, _mediaStore, AllSongsOfPlaylist);
            AddSong = new AddSongAsyncCommand(_musicService, _mediaStore, _playlistBrowserNavigationStore, AllSongsOfPlaylist);
            OnPropertyChanged(nameof(AllSongsOfPlaylist));

        }

        private void OnMusicPlayerEvent(object? sender, MusicPlayerEventArgs e)
        {
            switch (e.Type)
            {
                case PlayerEventType.Playing:
                    var songPlay = AllSongsOfPlaylist?.FirstOrDefault(x => x.Id == e.Media?.Id);
                    if (songPlay != null)
                    {
                        songPlay.Playing = true;
                    }
                    break;
                default:
                    var songStopped = AllSongsOfPlaylist?.FirstOrDefault(x => x.Id == e.Media?.Id);
                    if (songStopped != null)
                    {
                        songStopped.Playing = false;
                    }
                    break;
            }
        }

        private void OnPlaylistSongsAdded(object? sender, PlaylistSongsAddedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    foreach (MediaEntity mediaEntity in e.Songs)
                    {
                        if (mediaEntity.PlayerlistId == _playlistBrowserNavigationStore.BrowserPlaylistId)
                        {
                            string dbPath = mediaEntity.FilePath ?? string.Empty; 
                            string destinationPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? AppDomain.CurrentDomain.BaseDirectory) ?? string.Empty, "songs", Path.GetFileName(dbPath) ?? string.Empty);

                            if (!File.Exists(destinationPath) && File.Exists(dbPath))
                            {
                                try
                                {
                                    File.Copy(dbPath, destinationPath);
                                }
                                catch { /* Ignore file copy error */ }
                            }
                            
                            var songsIndex = AllSongsOfPlaylist?.Count;
                            AllSongsOfPlaylist?.Add(new MediaModel
                            {
                                Playing = _musicService.PlayerState == PlaybackState.Playing && mediaEntity.Id == _musicService.CurrentMedia?.Id,
                                Number = songsIndex + 1,
                                Id = mediaEntity.Id,
                                Title = Path.GetFileNameWithoutExtension(dbPath),
                                Path = dbPath, 
                                Duration = AudioUtills.DurationParse(dbPath)
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't crash
                    Debug.WriteLine($"Error adding song to playlist UI: {ex.Message}");
                }
            });
        }

        private void OnPlaylistBannerChange(object? sender, PlaylistBannerChangedArgs args)
        {
            BannerUrl = args.Banner;
        }

        public async Task OnFilesDroppedAsync(string[] files, object? parameter)
        {
            var mediaEntities = files.Where(x => PathExtension.HasAudioVideoExtensions(x)).Select(x => new MediaEntity
            {
                PlayerlistId = _playlistBrowserNavigationStore.BrowserPlaylistId,
                FilePath = x
            }).ToList();

            // Just add to store. The Event Handler will update the UI.
            await _mediaStore.AddRange(mediaEntities, true);
        }


        public override void Dispose()
        {
            _musicService.MusicPlayerEvent -= OnMusicPlayerEvent;
            _mediaStore.PlaylistSongsAdded -= OnPlaylistSongsAdded;
            _playlistStore.PlaylistBannerChanged -= OnPlaylistBannerChange;


        }
    }
}
