using ETMPClient.Extensions;
using ETMPClient.Models;
using ETMPClient.Services;
using ETMPClient.Stores;
using ETMPData.DataEntities;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace ETMPClient.Commands
{
    public class AddSongAsyncCommand : AsyncCommandBase
    {
        private readonly IMusicPlayerService _musicService;
        private readonly MediaStore _mediaStore;
        private readonly PlaylistBrowserNavigationStore _playlistBrowserNavigationStore;
        private readonly ObservableCollection<MediaModel>? _observableSongs;

        public AddSongAsyncCommand(IMusicPlayerService musicService, MediaStore mediaStore, PlaylistBrowserNavigationStore playlistBrowserNavigationStore)
        {
            _mediaStore = mediaStore;
            _musicService = musicService;
            _playlistBrowserNavigationStore = playlistBrowserNavigationStore;
        }
        public AddSongAsyncCommand(IMusicPlayerService musicService, MediaStore mediaStore, PlaylistBrowserNavigationStore playlistBrowserNavigationStore, ObservableCollection<MediaModel> observableSongs): this(musicService,mediaStore,playlistBrowserNavigationStore)
        {
            _observableSongs = observableSongs;
        }

        protected override async Task ExecuteAsync(object? parameter)
        {
            var openFileDialog = new OpenFileDialog();

            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;

                var songEntity = new MediaEntity
                {
                    PlayerlistId = _playlistBrowserNavigationStore.BrowserPlaylistId,
                    FilePath = fileName, // Store original path, let event handler copy if needed
                };

                // Use AddRange with event trigger to ensure UI updates via event handler
                await _mediaStore.AddRange(new[] { songEntity }, true);
            }
        }

    }
}
