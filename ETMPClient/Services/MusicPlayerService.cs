using Microsoft.EntityFrameworkCore;
using ETMPData.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using ETMPData.DataEntities;
using System.IO;
using System.Diagnostics;
using NAudio.Wave.SampleProviders;
using ETMPClient.Events;
using ETMPClient.Stores;
using ETMPClient.Enums;
using ETMPClient.Audio;

namespace ETMPClient.Services
{
    public interface IMusicPlayerService
    {
        public event EventHandler<MusicPlayerEventArgs>? MusicPlayerEvent;
        public event EventHandler? AfterMusicPlayerEvent;
        public string PlayingSongPath { get; }
        public string PlayingSongName { get; }
        public MediaEntity? CurrentMedia { get; }
        public PlaybackState PlayerState { get; }
        public float Volume { get; set; }
        public long Position { get; set; }
        public long TotalTime { get; }
        public void Play(int mediaId);
        public void Stop();
        public void RePlay();
        public void PlayPause();
        public void PlayNext(bool callStoppedPlay = true);
        public void PlayPrevious();
        public float VisualizerSensitivity { get; set; }
        public string VisualizerColorHex { get; set; }
        public double VisualizerOpacity { get; set; }
        public double VisualizerBrightness { get; set; }
        public double VisualizerBarThickness { get; set; }
        public double VisualizerSpeed { get; set; }
        public double HomeTitleFontSize { get; set; }
        public double HomeArtistFontSize { get; set; }
        public double HomeCornerRadius { get; set; }
        
        // Equalizer
        public string CurrentEqPreset { get; set; }
        public float[] EqualizerBands { get; }
        public void SetEqualizerBand(int bandIndex, float gainDb);
        public void ApplyEqualizerPreset(string presetName);
        public void ResetEqualizer();
        
        // Shuffle/Repeat
        public bool IsShuffleEnabled { get; set; }
        public RepeatMode RepeatMode { get; set; }
        public void ToggleShuffle();
        public void CycleRepeatMode();

        // Visualizer
        public float[] GetVisualizerData();

        public void ChangeVolume(float volume);
        public void Pause();
        public void Resume();
    }

    public class MusicPlayerService : IMusicPlayerService
    {
        private readonly MediaStore _mediaStore;
        private IWavePlayer _waveOutDevice;
        private AudioFileReader? _audioFileReader;
        private IWaveProvider? _audioFile;
        private EqualizerSampleProvider? _equalizer;
        private MediaEntity? _currentMedia;
        
        public MediaEntity? CurrentMedia => _currentMedia;

        public event EventHandler<MusicPlayerEventArgs>? MusicPlayerEvent;
        public event EventHandler? AfterMusicPlayerEvent;

        public float VisualizerSensitivity { get; set; } = 1.0f;
        public string VisualizerColorHex { get; set; } = "#1DB954"; // Default Green/Accent
        public double VisualizerOpacity { get; set; } = 0.95;
        public double VisualizerBrightness { get; set; } = 1.0; // 1.0 = normal, >1.0 = brighter
        public double VisualizerBarThickness { get; set; } = 4.0; // Bar width in pixels
        public double VisualizerSpeed { get; set; } = 0.3; // 0.1 (slow/smooth) to 1.0 (instant/fast)
        public double HomeTitleFontSize { get; set; } = 36;
        public double HomeArtistFontSize { get; set; } = 24;
        public double HomeCornerRadius { get; set; } = 20;

        // Equalizer properties
        public string CurrentEqPreset { get; set; } = "Flat";
        public float[] EqualizerBands { get; private set; } = new float[10];
        
        // Shuffle/Repeat properties
        public bool IsShuffleEnabled { get; set; } = false;
        public RepeatMode RepeatMode { get; set; } = RepeatMode.Off;
        private List<int> _shuffleQueue = new List<int>();
        private int _currentShuffleIndex = -1;
        
        // FFT Visualizer
        private SampleAggregator? _sampleAggregator;
        private float[] _fftValues = new float[32]; // 32 bars
        private readonly object _fftLock = new object();

        public float Volume
        {
            get => _waveOutDevice?.Volume ?? 0;
            set
            {
                if(_waveOutDevice != null && value >= 0 && value <= 1)
                {
                    _waveOutDevice.Volume = value;
                }
            }
        }

        public long Position
        {
            get
            {
                var pos = _audioFileReader?.Position / _audioFileReader?.WaveFormat.AverageBytesPerSecond ?? 0;
                if (TotalTime < pos)
                    return TotalTime;

                return pos;
            }
            set
            {
                if (_audioFileReader != null)
                {
                    _audioFileReader.Position = (long)(_audioFileReader.WaveFormat.AverageBytesPerSecond * value);
                }
            }
        }

        public long TotalTime
        {
            get => _audioFileReader?.Length / _audioFileReader?.WaveFormat.AverageBytesPerSecond ?? 0;
        }

        public string PlayingSongPath => _currentMedia?.FilePath ?? "";

        public string PlayingSongName => Path.GetFileNameWithoutExtension(_currentMedia?.FilePath) ?? "";

        public PlaybackState PlayerState => _waveOutDevice?.PlaybackState ?? PlaybackState.Stopped;

        public MusicPlayerService(MediaStore mediaStore)
        {
            _mediaStore = mediaStore;
            _waveOutDevice = new WaveOut();
            _waveOutDevice.PlaybackStopped += OnStoppedPlay;
        }

        public void ChangeVolume(float volume)
        {
            throw new NotImplementedException();
        }

        public void PlayPause()
        {
            if (_waveOutDevice == null) return;
            if (_waveOutDevice.PlaybackState == PlaybackState.Paused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
        
        public void Pause()
        {
            _waveOutDevice?.Pause();
            OnPausePlay();
        }

        public void Resume()
        {
            _waveOutDevice?.Play();
            OnStartPlay();
        }
        public void Play(int mediaId)
        {
            OnPausePlay();
            _currentMedia = _mediaStore.Songs.FirstOrDefault(x => x.Id == mediaId);
            if (_currentMedia != null)
            {
                _waveOutDevice?.Stop();
                _waveOutDevice?.Dispose();

                try
                {
                    _audioFileReader = new AudioFileReader(_currentMedia.FilePath);
                    
                    // Initialize FFT aggregator
                    _sampleAggregator = new SampleAggregator(1024);
                    _sampleAggregator.FftCalculated += OnFftCalculated;
                    
                    // Create a sample provider that captures samples for FFT
                    var sampleProvider = _audioFileReader.ToSampleProvider();
                    var capturingSampleProvider = new CapturingSampleProvider(sampleProvider, _sampleAggregator);
                    
                    // Create equalizer and insert into audio chain
                    _equalizer = new EqualizerSampleProvider(capturingSampleProvider);
                    
                    // Apply current preset
                    if (!string.IsNullOrEmpty(CurrentEqPreset) && EqualizerPresets.Presets.ContainsKey(CurrentEqPreset))
                    {
                        var presetValues = EqualizerPresets.Presets[CurrentEqPreset];
                        for (int i = 0; i < 10; i++)
                        {
                            _equalizer.SetBandGain(i, presetValues[i]);
                            EqualizerBands[i] = presetValues[i];
                        }
                    }
                    
                    // Convert back to IWaveProvider for WaveOut
                    _audioFile = _equalizer.ToWaveProvider();
                    
                    _waveOutDevice = new WaveOut();
                    _waveOutDevice.PlaybackStopped += OnStoppedPlay;
                    _waveOutDevice.Init(_audioFile);
                    _waveOutDevice.Play();
                    OnStartPlay();
                }
                catch
                {
                    Stop();
                }
            }
        }

        public void RePlay()
        {
            if (_audioFile != null)
            {
                _waveOutDevice?.Stop();
                _waveOutDevice?.Dispose();

                _waveOutDevice = new WaveOut();
                _waveOutDevice.PlaybackStopped += OnStoppedPlay;
                Position = 0;
                _waveOutDevice.Init(_audioFile);
                _waveOutDevice.Play();
                OnStartPlay();
            }
        }

        public void Stop()
        {
            _currentMedia = null;
            _audioFileReader?.Dispose();
            _audioFileReader = null;
            (_audioFile as IDisposable)?.Dispose();
            _audioFile = null;
            _waveOutDevice?.Stop();
            _waveOutDevice?.Dispose();

            OnStoppedPlay(this, null);

            _waveOutDevice = new WaveOut();
            _waveOutDevice.PlaybackStopped += OnStoppedPlay;
        }

        public void PlayNext(bool callStoppedPlay = true)
        {
            if (_currentMedia == null) return;

            // Repeat One: replay current song
            if (RepeatMode == RepeatMode.One)
            {
                RePlay();
                return;
            }

            MediaEntity? nextMedia = null;

            // Shuffle mode
            if (IsShuffleEnabled)
            {
                nextMedia = GetNextShuffled();
            }
            else
            {
                // Normal sequential playback
                nextMedia = _mediaStore.Songs.FirstOrDefault(x => 
                    x.Id > _currentMedia.Id && 
                    _currentMedia.PlayerlistId == x.PlayerlistId);
            }

            // Repeat All: loop to beginning
            if (nextMedia == null && RepeatMode == RepeatMode.All)
            {
                if (IsShuffleEnabled)
                {
                    // Reshuffle and start over
                    _shuffleQueue.Clear();
                    nextMedia = GetNextShuffled();
                }
                else
                {
                    // Go to first song in playlist
                    nextMedia = _mediaStore.Songs
                        .Where(x => _currentMedia.PlayerlistId == x.PlayerlistId)
                        .OrderBy(x => x.Id)
                        .FirstOrDefault();
                }
            }

            if (nextMedia != null)
            {
                Play(nextMedia.Id);
            }
            else if (RepeatMode == RepeatMode.Off)
            {
                Stop();
            }
        }

        private MediaEntity? GetNextShuffled()
        {
            // Build shuffle queue if empty
            if (_shuffleQueue.Count == 0)
            {
                var playlistSongs = _mediaStore.Songs
                    .Where(x => x.PlayerlistId == _currentMedia!.PlayerlistId)
                    .Select(x => x.Id)
                    .ToList();
                
                // Shuffle using Random
                var rng = new Random();
                _shuffleQueue = playlistSongs.OrderBy(x => rng.Next()).ToList();
                _currentShuffleIndex = _shuffleQueue.IndexOf(_currentMedia!.Id);
            }

            _currentShuffleIndex++;
            if (_currentShuffleIndex >= _shuffleQueue.Count)
            {
                return null; // End of shuffle queue
            }

            return _mediaStore.Songs.FirstOrDefault(x => x.Id == _shuffleQueue[_currentShuffleIndex]);
        }

        public void PlayPrevious()
        {
            if (_currentMedia != null)
            {
                Prev:
                var tempmedia = _mediaStore.Songs.Reverse().FirstOrDefault(x => x.Id < _currentMedia.Id && _currentMedia.PlayerlistId == x.PlayerlistId);
                if (tempmedia != null)
                {
                    _waveOutDevice?.Stop();
                    _waveOutDevice?.Dispose();

                    OnStoppedPlay(this, null);

                    _currentMedia = tempmedia;

                    try
                    {
                        _audioFileReader = new AudioFileReader(_currentMedia.FilePath);
                        
                        // Create equalizer and insert into audio chain
                        _equalizer = new EqualizerSampleProvider(_audioFileReader.ToSampleProvider());
                        
                        // Apply current preset
                        if (!string.IsNullOrEmpty(CurrentEqPreset) && EqualizerPresets.Presets.ContainsKey(CurrentEqPreset))
                        {
                            var presetValues = EqualizerPresets.Presets[CurrentEqPreset];
                            for (int i = 0; i < 10; i++)
                            {
                                _equalizer.SetBandGain(i, presetValues[i]);
                                EqualizerBands[i] = presetValues[i];
                            }
                        }
                        
                        // Convert back to IWaveProvider for WaveOut
                        _audioFile = _equalizer.ToWaveProvider();
                        
                        _waveOutDevice = new WaveOut();
                        _waveOutDevice.PlaybackStopped += OnStoppedPlay;
                        _waveOutDevice.Init(_audioFile);
                        _waveOutDevice.Play();
                        OnStartPlay();
                    }
                    catch
                    {
                        if (_mediaStore.Songs.FirstOrDefault(x => x.Id < _currentMedia.Id && _currentMedia.PlayerlistId == x.PlayerlistId) == null)
                        {
                            Stop();
                        }
                        else
                        {
                            goto Prev;
                        }
                    }
                }
            }
        }

        private void OnStoppedPlay(object? sender, StoppedEventArgs? e)
        {
            if (e == null)
            {
                MusicPlayerEvent?.Invoke(this, new MusicPlayerEventArgs(PlayerEventType.Stopped, _currentMedia, _audioFile));
            }
            else
            {
                MusicPlayerEvent?.Invoke(this, new MusicPlayerEventArgs(PlayerEventType.Finished, _currentMedia, _audioFile));
            }
            OnAfterPlay();
        }

        private void OnStartPlay()
        {
            MusicPlayerEvent?.Invoke(this, new MusicPlayerEventArgs(PlayerEventType.Playing, _currentMedia, _audioFile));
            OnAfterPlay();
        }

        private void OnPausePlay()
        {
            MusicPlayerEvent?.Invoke(this, new MusicPlayerEventArgs(PlayerEventType.Paused, _currentMedia, _audioFile));
            OnAfterPlay();
        }

        private void OnAfterPlay()
        {
            AfterMusicPlayerEvent?.Invoke(this, new EventArgs());
        }

        // Equalizer Methods
        public void SetEqualizerBand(int bandIndex, float gainDb)
        {
            if (bandIndex < 0 || bandIndex >= 10)
                return;

            EqualizerBands[bandIndex] = gainDb;
            _equalizer?.SetBandGain(bandIndex, gainDb);
        }

        public void ApplyEqualizerPreset(string presetName)
        {
            if (!EqualizerPresets.Presets.ContainsKey(presetName))
                return;

            CurrentEqPreset = presetName;
            var presetValues = EqualizerPresets.Presets[presetName];
            
            for (int i = 0; i < 10; i++)
            {
                SetEqualizerBand(i, presetValues[i]);
            }
        }

        public void ResetEqualizer()
        {
            ApplyEqualizerPreset("Flat");
        }
        
        public void ToggleShuffle()
        {
            IsShuffleEnabled = !IsShuffleEnabled;
            _shuffleQueue.Clear(); // Reset shuffle queue when toggling
            _currentShuffleIndex = -1;
        }

        public void CycleRepeatMode()
        {
            RepeatMode = RepeatMode switch
            {
                RepeatMode.Off => RepeatMode.All,
                RepeatMode.All => RepeatMode.One,
                RepeatMode.One => RepeatMode.Off,
                _ => RepeatMode.Off
            };
        }
        
        public float[] GetVisualizerData()
        {
            lock (_fftLock)
            {
                return (float[])_fftValues.Clone();
            }
        }

        private void OnFftCalculated(object? sender, FftEventArgs e)
        {
            lock (_fftLock)
            {
                // Convert FFT complex values to 32 bars
                int barCount = 32;
                int samplesPerBar = e.Result.Length / (barCount * 2); // Use first half of FFT

                for (int i = 0; i < barCount; i++)
                {
                    double sum = 0;
                    int startIndex = i * samplesPerBar;
                    int endIndex = Math.Min(startIndex + samplesPerBar, e.Result.Length / 2);

                    for (int j = startIndex; j < endIndex; j++)
                    {
                        double magnitude = Math.Sqrt(e.Result[j].X * e.Result[j].X + e.Result[j].Y * e.Result[j].Y);
                        sum += magnitude;
                    }

                    double average = sum / samplesPerBar;
                    // Massively increased scaling for better visibility: 5000x, max 200, minimum 5
                    float value = (float)Math.Min(average * 5000 * VisualizerSensitivity, 200);
                    _fftValues[i] = Math.Max(value, 5); // Minimum 5 for visibility
                }
            }
        }
    }
}
