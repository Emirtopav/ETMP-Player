using ETMPClient.Services;
using System;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ETMPClient.Commands;
using NAudio.Wave;
using ETMPClient.Events;
using ETMPClient.Core;
using ETMPClient.Extensions;
using ETMPClient.Enums;

namespace ETMPClient.ViewModels
{
    public class PlayerViewModel : ViewModelBase
    {
        private readonly IMusicPlayerService _musicService;

        private bool _playNext;

        public int Volume
        {
            get => (int)Math.Ceiling(_musicService.Volume * 100);
            set
            {
                _musicService.Volume = value / 100f;
                OnPropertyChanged();
            }
        }

        private string? _songName;
        public string? SongName
        {
            get => _songName;
            set
            {
                _songName = value;
                OnPropertyChanged();
            }
        }

        private string? _songPath;
        public string? SongPath
        {
            get => _songPath;
            set
            {
                _songPath = value;
                OnPropertyChanged();
            }
        }

        private string? _artistName;
        public string? ArtistName
        {
            get => _artistName;
            set
            {
                _artistName = value;
                OnPropertyChanged();
            }
        }

        private byte[]? _coverArt;
        public byte[]? CoverArt
        {
            get => _coverArt;
            set
            {
                _coverArt = value;
                OnPropertyChanged();
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

        public long SongProgress
        {
            get => _musicService.Position;
            set
            {
                _musicService.Position = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SongProgressFormatted));
            }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsShuffleEnabled => _musicService.IsShuffleEnabled;
        public RepeatMode RepeatMode => _musicService.RepeatMode;

        public string SongProgressFormatted => AudioUtills.DurationParse(SongProgress);

        public long SongDuration => _musicService.TotalTime;

        public string SongDurationFormatted => AudioUtills.DurationParse(SongDuration);

        public ICommand TogglePlayer { get; }
        public ICommand PlayForward { get; }
        public ICommand PlayBackward { get; }
        public ICommand ToggleShuffleCommand { get; }
        public ICommand CycleRepeatCommand { get; }
        public ICommand OpenExplorer { get; }
        public ICommand ToggleVolume { get; }

        public PlayerViewModel(IMusicPlayerService musicService)
        {
            _musicService = musicService;
            PlayBackward = new BackwardSongCommand(musicService);
            PlayForward = new ForwardSongCommand(musicService);
            TogglePlayer = new ToggleMusicPlayerStateCommand(musicService);
            OpenExplorer = new OpenExplorerAtPathCommand();
            ToggleVolume = new ToggleVolumeCommand(this);
            
            ToggleShuffleCommand = new RelayCommand(_ => {
                _musicService.ToggleShuffle();
                OnPropertyChanged(nameof(IsShuffleEnabled));
            });
            
            CycleRepeatCommand = new RelayCommand(_ => {
                _musicService.CycleRepeatMode();
                OnPropertyChanged(nameof(RepeatMode));
            });

            _musicService.MusicPlayerEvent += OnMusicPlayerEvent;
            _musicService.AfterMusicPlayerEvent += OnAfterMusicPlayerEvent;

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(500);
            dispatcherTimer.Start();
            
            InitializeVisualizer();
        }

        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(SongProgress));
            OnPropertyChanged(nameof(SongDuration));
            OnPropertyChanged(nameof(SongProgressFormatted));
            OnPropertyChanged(nameof(SongDurationFormatted));
            OnPropertyChanged(nameof(VisualizerOpacity));
            OnPropertyChanged(nameof(VisualizerColorHex));
            OnPropertyChanged(nameof(VisualizerBrightness));
            OnPropertyChanged(nameof(IsShuffleEnabled));
            OnPropertyChanged(nameof(RepeatMode));
        }

        private void OnMusicPlayerEvent(object? sender, MusicPlayerEventArgs e)
        {
            _playNext = false;
            switch (e.Type)
            {
                case PlayerEventType.Playing:
                    IsPlaying = true;
                    break;
                case PlayerEventType.Finished:
                    IsPlaying = false;
                    _playNext = true;
                    break;
                default:
                    IsPlaying = false;
                    break;
            }

            SongName = _musicService.PlayingSongName;
            SongPath = _musicService.PlayingSongPath;
            
            if (_musicService.CurrentMedia != null)
            {
                CoverArt = _musicService.CurrentMedia.CoverArtData;
                ArtistName = _musicService.CurrentMedia.Artist;
            }
            else
            {
                CoverArt = null;
                ArtistName = null;
            }
        }

        private void OnAfterMusicPlayerEvent(object? sender, EventArgs args)
        {
            if (_playNext)
            {
                _musicService.PlayNext(false);
                _playNext = false;
            }
        }
        
        // Visualizer Logic
        private System.Collections.ObjectModel.ObservableCollection<ETMPClient.Models.VisualBarViewModel> _visualizerBands = new();
        public System.Collections.ObjectModel.ObservableCollection<ETMPClient.Models.VisualBarViewModel> VisualizerBands
        {
            get => _visualizerBands;
            set
            {
                _visualizerBands = value;
                OnPropertyChanged(nameof(VisualizerBands));
            }
        }
        
        private readonly Random _rng = new Random();
        private DispatcherTimer _visTimer = new();

        private void InitializeVisualizer()
        {
            // The VisualizerBands property is already initialized by the field initializer.
            // This line is no longer needed if the collection is initialized at declaration.
            // VisualizerBands = new System.Collections.ObjectModel.ObservableCollection<ETMPClient.Models.VisualBarViewModel>();
            for(int i=0; i<64; i++)
            {
                VisualizerBands.Add(new ETMPClient.Models.VisualBarViewModel { Value = 5 });
            }

            _visTimer = new DispatcherTimer();
            _visTimer.Interval = TimeSpan.FromMilliseconds(30);
            _visTimer.Tick += (s, e) => UpdateVisualizer();
            _visTimer.Start();
        }

        private void UpdateVisualizer()
        {
            if (!IsPlaying)
            {
                // Reset to low
                foreach(var bar in VisualizerBands)
                {
                    if(bar.Value > 2) bar.Value -= 2;
                }
                return;
            }

            // Get real FFT data from music service
            var fftData = _musicService.GetVisualizerData();
            
            // Update first 32 bars with FFT data
            for (int i = 0; i < Math.Min(32, VisualizerBands.Count); i++)
            {
                if (i < fftData.Length)
                {
                    // Smooth transition with adjustable speed
                    double targetValue = fftData[i];
                    double currentValue = VisualizerBands[i].Value;
                    double speed = _musicService.VisualizerSpeed;
                    VisualizerBands[i].Value = currentValue + (targetValue - currentValue) * speed;
                }
            }
            
            // Mirror for remaining bars (32-64) for symmetry
            for (int i = 32; i < VisualizerBands.Count && i < 64; i++)
            {
                int mirrorIndex = 63 - i; // Mirror from end
                if (mirrorIndex < fftData.Length)
                {
                    double targetValue = fftData[mirrorIndex];
                    double currentValue = VisualizerBands[i].Value;
                    double speed = _musicService.VisualizerSpeed;
                    VisualizerBands[i].Value = currentValue + (targetValue - currentValue) * speed;
                }
            }
        }

        public override void Dispose()
        {
            _musicService.MusicPlayerEvent -= OnMusicPlayerEvent;
            _musicService.AfterMusicPlayerEvent -= OnAfterMusicPlayerEvent;
        }
    }
}
