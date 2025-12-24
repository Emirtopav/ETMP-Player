using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Common;
using System;
using System.Linq;

namespace ETMPClient.Services
{
    public class MidiPlayerService : IDisposable
    {
        private MidiFile? _midiFile;
        private Playback? _playback;
        private OutputDevice? _outputDevice;
        private bool _isPlaying;
        private bool _isPaused;

        public event EventHandler? PlaybackStopped;
        public event EventHandler<TimeSpan>? PositionChanged;
        public event EventHandler<int[]>? InstrumentsDetected;

        public bool IsPlaying => _isPlaying;
        public bool IsPaused => _isPaused;
        
        public TimeSpan Duration { get; private set; }
        public TimeSpan Position 
        { 
            get
            {
                if (_playback == null) return TimeSpan.Zero;
                var currentTime = _playback.GetCurrentTime<MetricTimeSpan>();
                return (TimeSpan)currentTime;
            }
        }

        public void LoadFile(string filePath)
        {
            Stop();
            
            try
            {
                // Load MIDI file using DryWetMIDI
                _midiFile = MidiFile.Read(filePath);
                
                // Get duration
                var duration = _midiFile.GetDuration<MetricTimeSpan>();
                Duration = (TimeSpan)duration;
                
                // Get first available MIDI output device (system synthesizer)
                var deviceName = OutputDevice.GetAll().FirstOrDefault()?.Name;
                if (deviceName == null)
                {
                    throw new Exception("No MIDI output device found");
                }
                
                _outputDevice = OutputDevice.GetByName(deviceName);
                
                // Create playback
                _playback = _midiFile.GetPlayback(_outputDevice);
                
                // Subscribe to events
                _playback.Finished += OnPlaybackFinished;
                
                // Detect instruments
                DetectInstruments();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load MIDI file: {ex.Message}", ex);
            }
        }

        private void DetectInstruments()
        {
            if (_midiFile == null) return;

            var instruments = new int[16];
            for (int i = 0; i < 16; i++)
            {
                instruments[i] = -1; // -1 means no instrument detected
            }

            // Read all Program Change events to detect instruments per channel
            foreach (var trackChunk in _midiFile.GetTrackChunks())
            {
                foreach (var midiEvent in trackChunk.Events)
                {
                    if (midiEvent is ProgramChangeEvent programChange)
                    {
                        var channel = (int)programChange.Channel;
                        var program = (int)programChange.ProgramNumber;
                        instruments[channel] = program;
                    }
                }
            }

            // Notify listeners
            InstrumentsDetected?.Invoke(this, instruments);
        }

        private void OnPlaybackFinished(object? sender, EventArgs e)
        {
            _isPlaying = false;
            _isPaused = false;
            PlaybackStopped?.Invoke(this, EventArgs.Empty);
        }

        public void Play()
        {
            if (_playback == null) return;

            if (_isPaused)
            {
                Resume();
                return;
            }

            // Start playback
            _playback.Start();
            _isPlaying = true;
            _isPaused = false;
        }

        public void Pause()
        {
            if (_isPlaying && !_isPaused && _playback != null)
            {
                _playback.Stop();
                _isPaused = true;
                _isPlaying = false;
            }
        }

        public void Resume()
        {
            if (_isPaused && _playback != null)
            {
                _playback.Start();
                _isPaused = false;
                _isPlaying = true;
            }
        }

        public void Stop()
        {
            _isPlaying = false;
            _isPaused = false;
            
            if (_playback != null)
            {
                try
                {
                    _playback.Stop();
                    _playback.Dispose();
                }
                catch { }
                _playback = null;
            }
            
            if (_outputDevice != null)
            {
                try
                {
                    _outputDevice.Dispose();
                }
                catch { }
                _outputDevice = null;
            }
        }

        public void SetChannelVolume(int channel, int volume)
        {
            if (_outputDevice == null) return;

            try
            {
                // Limit volume to 0-127
                volume = Math.Clamp(volume, 0, 127);
                
                // Channel 0-15
                channel = Math.Clamp(channel, 0, 15);

                var controlChange = new ControlChangeEvent((SevenBitNumber)7, (SevenBitNumber)volume)
                {
                    Channel = (FourBitNumber)channel
                };

                _outputDevice.SendEvent(controlChange);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set volume: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Stop();
            _midiFile = null;
        }
    }
}
