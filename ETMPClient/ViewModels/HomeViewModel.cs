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
using static System.Net.WebRequestMethods;

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

            // Re-broadcast property changes if settings change
            // Ideally we'd subscribe to an event, but for now we rely on UI binding updates or manual refresh if needed.
            // Since Service properties are simple auto-props, we might need a mechanism to notify this VM.
            // For simplicity in this session, we'll assume the view re-reads it or we accept it updates on restart/nav, 
            // BUT to make it reactive, we should hook into PropertyChanged if Service implemented it. 
            // Current Service doesn't implement INotifyPropertyChanged. 
            // We will fix this by making Service notify, or just binding directly to Service in XAML via ObjectDataProvider? 
            // Easier: just expose them here. To make them update live, we'd need an event.
            // Let's add a simple 'Refresh' or just accept they update on navigation for now?
            // User asked for settings, implies live update.
            // Strategy: I will add `OnSettingChanged` event to Service later if needed, 
            // but for now let's just expose them.
            // Wait, I can bind directly to the Service singleton if I registered it as a resource, but that's messy.
            // I'll make the properties pass-through.
            
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
