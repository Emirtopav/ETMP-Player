using System;

namespace ETMPClient.ViewModels
{
    using Microsoft.Win32;
    using ETMPClient.Commands;
    using ETMPClient.Services;
    using ETMPClient.Stores;
    using ETMPData.Data;
    using ETMPData.DataEntities;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class LibraryViewModel : ViewModelBase
    {
        private readonly MediaStore _mediaStore;
        private readonly IMusicPlayerService _musicPlayerService;

        public ObservableCollection<MediaEntity> Songs { get; set; }
        public ObservableCollection<MediaEntity> FilteredSongs { get; set; }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterSongs();
            }
        }

        public ICommand AddMusicCommand { get; }
        public ICommand PlaySongCommand { get; }
        public ICommand DeleteSongCommand { get; }

        public LibraryViewModel(MediaStore mediaStore, IMusicPlayerService musicPlayerService)
        {
            try
            {
                _mediaStore = mediaStore;
                _musicPlayerService = musicPlayerService;

                Songs = new ObservableCollection<MediaEntity>(_mediaStore.Songs);
                FilteredSongs = new ObservableCollection<MediaEntity>(_mediaStore.Songs);

                AddMusicCommand = new RelayCommand(AddMusic);
                PlaySongCommand = new RelayCommand<MediaEntity>(PlaySong);
                DeleteSongCommand = new RelayCommand<MediaEntity>(DeleteSong);

                _ = LoadMetadataAsync();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("library_crash.txt", $"LibraryViewModel Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}\n\nInner Exception: {ex.InnerException?.Message}\n{ex.InnerException?.StackTrace}");
                throw;
            }
        }

        private void FilterSongs()
        {
            FilteredSongs.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var song in Songs)
                {
                    FilteredSongs.Add(song);
                }
                return;
            }

            var query = SearchText.ToLower();
            var filtered = Songs.Where(s =>
                (s.Title?.ToLower().Contains(query) ?? false) ||
                (s.Artist?.ToLower().Contains(query) ?? false) ||
                (s.Album?.ToLower().Contains(query) ?? false));

            foreach (var song in filtered)
            {
                FilteredSongs.Add(song);
            }
        }

        private async Task LoadMetadataAsync()
        {
            await Task.Run(() =>
            {
                foreach (var media in Songs.ToList())
                {
                    try
                    {
                        if (System.IO.File.Exists(media.FilePath))
                        {
                            using (var tfile = TagLib.File.Create(media.FilePath))
                            {
                                var title = tfile.Tag.Title; // Read to local var
                                var artist = tfile.Tag.FirstPerformer;
                                var album = tfile.Tag.Album;
                                var duration = tfile.Properties.Duration;
                                byte[]? art = null;
                                if (tfile.Tag.Pictures.Length > 0)
                                    art = ETMPClient.Helpers.ImageHelper.TagLibPictureToBytes(tfile.Tag.Pictures[0]);

                                // Update UI on Main Thread
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (!string.IsNullOrEmpty(title)) media.Title = title;
                                    if (!string.IsNullOrEmpty(artist)) media.Artist = artist;
                                    if (!string.IsNullOrEmpty(album)) media.Album = album;
                                    if (art != null) media.CoverArtData = art;
                                    media.Duration = duration;
                                    
                                    // Trigger update if Title was fallback
                                    if(string.IsNullOrEmpty(title) && string.IsNullOrEmpty(media.Title))
                                    {
                                        // Force PropertyChanged if needed, but setter does it.
                                    }
                                });
                            }
                        }
                    }
                    catch { /* Ignore read errors */ }
                }
            });
        }

        private async void AddMusic(object obj)
        {
            var openFolderDialog = new OpenFolderDialog
            {
                Multiselect = false,
                Title = "Select Music Folder"
            };

            if (openFolderDialog.ShowDialog() == true)
            {
                var folderPath = openFolderDialog.FolderName;
                var supportedExtensions = new[] { ".mp3", ".wav", ".flac", ".m4a", ".wma", ".mid", ".midi" };
                
                // Get all audio files recursively
                var files = System.IO.Directory.GetFiles(folderPath, "*.*", System.IO.SearchOption.AllDirectories)
                                             .Where(f => supportedExtensions.Contains(System.IO.Path.GetExtension(f).ToLower()));

                var mediaEntities = files.Select(path => 
                {
                    var media = new MediaEntity { FilePath = path };
                    try
                    {
                        using (var tfile = TagLib.File.Create(path))
                        {
                            media.Title = tfile.Tag.Title ?? System.IO.Path.GetFileNameWithoutExtension(path);
                            media.Artist = tfile.Tag.FirstPerformer ?? "Unknown Artist";
                            media.Album = tfile.Tag.Album ?? "Unknown Album";
                            media.Duration = tfile.Properties.Duration;
                            
                            if (tfile.Tag.Pictures.Length > 0)
                            {
                                media.CoverArtData = ETMPClient.Helpers.ImageHelper.TagLibPictureToBytes(tfile.Tag.Pictures[0]);
                            }
                        }
                    }
                    catch
                    {
                        // Fallback in case of error reading tags
                        media.Title = System.IO.Path.GetFileNameWithoutExtension(path);
                        media.Artist = "Unknown Artist";
                    }
                    return media;
                }).ToList();

                if (mediaEntities.Any())
                {
                    await _mediaStore.AddRange(mediaEntities, true);
                    
                    // Reload MediaStore to ensure in-memory collection is updated
                    _mediaStore.Load();

                    // Update UI on UI thread
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        Songs.Clear();
                        foreach (var song in _mediaStore.Songs)
                        {
                            Songs.Add(song);
                        }
                        FilterSongs(); // Reapply filter after adding new songs
                    });
                }
            }
        }

        private void PlaySong(MediaEntity song)
        {
            if (song != null)
            {
                _musicPlayerService.Play(song.Id);
            }
        }

        private async void DeleteSong(MediaEntity song)
        {
            if (song != null)
            {
                // Stop playing if this song is currently playing
                if (_musicPlayerService.CurrentMedia?.Id == song.Id)
                {
                    _musicPlayerService.Stop();
                }

                // Remove from database
                await _mediaStore.Remove(song.Id);

                // Remove from UI
                Songs.Remove(song);
                FilteredSongs.Remove(song);
            }
        }
    }
}
