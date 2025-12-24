using ETMPClient.Commands;
using ETMPClient.Core;
using ETMPClient.Models;
using ETMPClient.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using System.ComponentModel;

namespace ETMPClient.ViewModels
{
    public class MidiViewModel : ViewModelBase
    {
        private readonly MidiPlayerService _midiPlayer;
        private ObservableCollection<MidiFileInfo> _midiFiles = new();
        private MidiFileInfo? _currentMidi;
        private ObservableCollection<ChannelControl> _channels = new();
        private bool _isPlaying;
        private TimeSpan _position;
        private TimeSpan _duration;

        public ObservableCollection<MidiFileInfo> MidiFiles
        {
            get => _midiFiles;
            set { _midiFiles = value; OnPropertyChanged(nameof(MidiFiles)); }
        }

        public MidiFileInfo? CurrentMidi
        {
            get => _currentMidi;
            set { _currentMidi = value; OnPropertyChanged(nameof(CurrentMidi)); }
        }

        public ObservableCollection<ChannelControl> Channels
        {
            get => _channels;
            set { _channels = value; OnPropertyChanged(nameof(Channels)); }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; OnPropertyChanged(nameof(IsPlaying)); }
        }

        public TimeSpan Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(nameof(Position)); }
        }

        public TimeSpan Duration
        {
            get => _duration;
            set { _duration = value; OnPropertyChanged(nameof(Duration)); }
        }

        public ICommand AddMidiFileCommand { get; }
        public ICommand PlayMidiCommand { get; }
        public ICommand RemoveMidiCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }

        public MidiViewModel(MidiPlayerService midiPlayer)
        {
            _midiPlayer = midiPlayer;
            
            // Initialize 16 MIDI channels
            for (int i = 0; i < 16; i++)
            {
                var channel = new ChannelControl
                {
                    ChannelNumber = i + 1,
                    Volume = 100
                };
                
                channel.PropertyChanged += OnChannelPropertyChanged;
                Channels.Add(channel);
            }

            // Subscribe to player events
            _midiPlayer.PlaybackStopped += (s, e) => IsPlaying = false;
            _midiPlayer.InstrumentsDetected += OnInstrumentsDetected;

            // Initialize commands
            AddMidiFileCommand = new RelayCommand(AddMidiFile);
            PlayMidiCommand = new RelayCommand<MidiFileInfo>(PlayMidi);
            RemoveMidiCommand = new RelayCommand<MidiFileInfo>(RemoveMidi);
            PlayCommand = new RelayCommand(Play);
            PauseCommand = new RelayCommand(Pause);
            StopCommand = new RelayCommand(Stop);
        }

        private void OnChannelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ChannelControl) return;

            // Handle Volume, Mute, and Solo changes
            if (e.PropertyName == nameof(ChannelControl.Volume) || 
                e.PropertyName == nameof(ChannelControl.IsMuted) ||
                e.PropertyName == nameof(ChannelControl.IsSolo))
            {
                UpdateAllChannelVolumes();
            }
        }

        private void UpdateAllChannelVolumes()
        {
            bool isAnySolo = Channels.Any(c => c.IsSolo);

            foreach (var channel in Channels)
            {
                int targetVolume = channel.Volume;

                // Priority Logic:
                // 1. If Active Solo exists globally AND this channel is NOT Solo -> Mute (Volume 0)
                // 2. If this channel is Muted -> Mute (Volume 0)
                // 3. Otherwise -> Current Volume

                if (isAnySolo && !channel.IsSolo)
                {
                    targetVolume = 0;
                }
                else if (channel.IsMuted)
                {
                    targetVolume = 0;
                }

                // Apply volume
                // MIDI channels are 0-indexed (0-15), model is 1-indexed
                _midiPlayer.SetChannelVolume(channel.ChannelNumber - 1, targetVolume);
            }
        }

        private void OnInstrumentsDetected(object? sender, int[] instruments)
        {
            for (int i = 0; i < Math.Min(instruments.Length, Channels.Count); i++)
            {
                if (instruments[i] >= 0)
                {
                    Channels[i].InstrumentName = MidiInstruments.GetInstrumentName(instruments[i]);
                }
                else
                {
                    Channels[i].InstrumentName = "No Instrument";
                }
            }
        }

        private void AddMidiFile(object? parameter)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "MIDI Files (*.mid;*.midi)|*.mid;*.midi",
                Multiselect = true,
                Title = "Select MIDI Files"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var file in openFileDialog.FileNames)
                {
                    if (!MidiFiles.Any(m => m.FilePath == file))
                    {
                        var midiInfo = new MidiFileInfo
                        {
                            FilePath = file,
                            FileName = Path.GetFileName(file)
                        };
                        
                        MidiFiles.Add(midiInfo);
                    }
                }
            }
        }

        private void PlayMidi(MidiFileInfo? midiInfo)
        {
            if (midiInfo == null) return;

            try
            {
                _midiPlayer.LoadFile(midiInfo.FilePath);
                CurrentMidi = midiInfo;
                Duration = _midiPlayer.Duration;
                _midiPlayer.Play();
                IsPlaying = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to play MIDI: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveMidi(MidiFileInfo? midiInfo)
        {
            if (midiInfo == null) return;

            if (CurrentMidi == midiInfo)
            {
                Stop(null);
            }
            MidiFiles.Remove(midiInfo);
        }

        private void Play(object? parameter)
        {
            if (CurrentMidi != null)
            {
                _midiPlayer.Play();
                IsPlaying = true;
            }
        }

        private void Pause(object? parameter)
        {
            _midiPlayer.Pause();
            IsPlaying = false;
        }

        private void Stop(object? parameter)
        {
            _midiPlayer.Stop();
            IsPlaying = false;
            Position = TimeSpan.Zero;
        }
    }
}
