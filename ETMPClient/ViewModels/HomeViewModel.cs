using Microsoft.EntityFrameworkCore;
using ETMPClient.Commands;
using ETMPClient.Services;
using ETMPData.Data;
using ETMPData.DataEntities;
using ETMPClient.Models;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ETMPClient.Events;
using System.Diagnostics;
using ETMPClient.Stores;
using ETMPClient.Interfaces;
using System.Windows;
using ETMPClient.Extensions;
using ETMPClient.Core;
using ETMPClient.Enums;

namespace ETMPClient.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly IMusicPlayerService _musicPlayerService;
        private string _songName = "No Song Playing";
        private string _artistName = "Select a song to start";

        public string SongName
        {
            get => _songName;
            set
            {
                _songName = value;
                OnPropertyChanged(nameof(SongName));
            }
        }

        public string ArtistName
        {
            get => _artistName;
            set
            {
                _artistName = value;
                OnPropertyChanged(nameof(ArtistName));
            }
        }

        public ICommand OpenLink { get; }

        private byte[]? _coverArt;
        public byte[]? CoverArt
        {
            get => _coverArt;
            set
            {
                _coverArt = value;
                OnPropertyChanged(nameof(CoverArt));
            }
        }

        public double HomeCornerRadius => _musicPlayerService.HomeCornerRadius;
        public double HomeTitleFontSize => _musicPlayerService.HomeTitleFontSize;
        public double HomeArtistFontSize => _musicPlayerService.HomeArtistFontSize;

        public HomeViewModel(IMusicPlayerService musicPlayerService)
        {
            _musicPlayerService = musicPlayerService;
            OpenLink = new OpenLinkCommand();
            
            _musicPlayerService.MusicPlayerEvent += OnMusicPlayerEvent;
            UpdateSongInfo();
        }

        private void OnMusicPlayerEvent(object? sender, MusicPlayerEventArgs e)
        {
             UpdateSongInfo();
        }

        private void UpdateSongInfo()
        {
            var media = _musicPlayerService.CurrentMedia;
            if (media != null)
            {
                SongName = !string.IsNullOrEmpty(media.Title) ? media.Title : Path.GetFileNameWithoutExtension(media.FilePath ?? "Unknown");
                ArtistName = !string.IsNullOrEmpty(media.Artist) ? media.Artist : "Unknown Artist";
                CoverArt = media.CoverArtData;
            }
            else
            {
                SongName = "ETMP";
                ArtistName = "Ready to Play";
                CoverArt = null;
            }
        }

        public override void Dispose()
        {
            _musicPlayerService.MusicPlayerEvent -= OnMusicPlayerEvent;
            base.Dispose();
        }
    }
}
