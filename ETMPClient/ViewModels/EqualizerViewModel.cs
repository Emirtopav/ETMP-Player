using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ETMPClient.Commands;
using ETMPClient.Services;
using ETMPClient.Audio;
using ETMPClient.Models;

namespace ETMPClient.ViewModels
{
    public class EqBand : ViewModelBase
    {
        private readonly IMusicPlayerService _musicService;
        private readonly int _bandIndex;

        public string Label { get; set; } = "";
        public int FrequencyBars { get; set; } = 3; // Number of bars to show in visualizer
        public double FrequencyBarHeight { get; set; } = 20; // Height of bars (higher freq = taller)
        
        private float _value;
        private bool _isAnimating = false; // Prevent service updates during animation
        public float Value
        {
            get => _value;
            set 
            { 
                if (_value != value)
                {
                    _value = value;
                    if (!_isAnimating) // Only update service if not animating
                    {
                        _musicService?.SetEqualizerBand(_bandIndex, value);
                    }
                    OnPropertyChanged();
                    // Trigger auto-save
                    if (!_isAnimating)
                    {
                        ValueChanged?.Invoke();
                    }
                }
            }
        }

        public event Action? ValueChanged;

        /// <summary>
        /// Smoothly animate to a new value
        /// </summary>
        public void AnimateToValue(float targetValue)
        {
            _isAnimating = true;
            var currentValue = _value;
            
            // Simple linear interpolation over time
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10) // 10ms updates
            };
            
            var startTime = DateTime.Now;
            var duration = TimeSpan.FromMilliseconds(120);
            
            timer.Tick += (s, e) =>
            {
                var elapsed = DateTime.Now - startTime;
                var progress = Math.Min(1.0, elapsed.TotalMilliseconds / duration.TotalMilliseconds);
                
                // Ease out cubic for smooth deceleration
                var easedProgress = 1 - Math.Pow(1 - progress, 3);
                
                var newValue = currentValue + (targetValue - currentValue) * (float)easedProgress;
                Value = newValue;
                
                if (progress >= 1.0)
                {
                    timer.Stop();
                    Value = targetValue; // Ensure exact final value
                    _isAnimating = false;
                    _musicService?.SetEqualizerBand(_bandIndex, targetValue);
                    ValueChanged?.Invoke();
                }
            };
            
            timer.Start();
        }

        public EqBand(string label, int bandIndex, IMusicPlayerService musicService, int frequencyBars, double barHeight)
        {
            Label = label;
            _bandIndex = bandIndex;
            _musicService = musicService;
            FrequencyBars = frequencyBars;
            FrequencyBarHeight = barHeight;
        }
    }

    public class VisualizerBand : ViewModelBase
    {
        private double _value;
        public double Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }

    public class EqualizerViewModel : ViewModelBase
    {
        private readonly IMusicPlayerService _musicService;
        private readonly PlayerViewModel _playerViewModel;
        private static readonly Random _random = new Random();
        
        public PlayerViewModel PlayerViewModel => _playerViewModel;
        
        public ObservableCollection<EqBand> Bands { get; } = new ObservableCollection<EqBand>();

        private string _selectedPreset = "Flat";
        public string SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                if (_selectedPreset != value)
                {
                    _selectedPreset = value;
                    OnPropertyChanged();
                    ApplyPreset(value);
                }
            }
        }

        public string[] Presets => EqualizerPresets.PresetNames;

        private float _preamp = 0;
        public float Preamp
        {
            get => _preamp;
            set
            {
                if (_preamp != value)
                {
                    _preamp = value;
                    OnPropertyChanged();
                    // TODO: Apply preamp to audio
                    SaveSettings(); // Auto-save
                }
            }
        }

        public ICommand ResetCommand { get; }

        public EqualizerViewModel(IMusicPlayerService musicService, PlayerViewModel playerViewModel)
        {
            try
            {
                _musicService = musicService;
                _playerViewModel = playerViewModel;
                ResetCommand = new RelayCommand(_ => ResetEq(null!));

                // Initialize 10 bands
                string[] labels = { "31 Hz", "62 Hz", "125 Hz", "250 Hz", "500 Hz", "1 kHz", "2 kHz", "4 kHz", "8 kHz", "16 kHz" };
                int[] barCounts = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 12 };
                double[] barHeights = { 15, 18, 21, 24, 27, 30, 33, 36, 39, 42 };
                
                for (int i = 0; i < 10; i++)
                {
                    Bands.Add(new EqBand(labels[i], i, _musicService, barCounts[i], barHeights[i]));
                }

                // Load current values from service
                for (int i = 0; i < 10; i++)
                {
                    Bands[i].Value = _musicService.EqualizerBands[i];
                }

                LoadSettings();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("equalizer_crash.txt", $"EqualizerViewModel Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}\n\nInner Exception: {ex.InnerException?.Message}\n{ex.InnerException?.StackTrace}");
                throw;
            }
        }


        private void ApplyPreset(string presetName)
        {
            _musicService.ApplyEqualizerPreset(presetName);
            
            // Animate UI to new values
            for (int i = 0; i < 10; i++)
            {
                Bands[i].AnimateToValue(_musicService.EqualizerBands[i]);
            }
            
            // Save after animation completes (delay slightly)
            Task.Delay(150).ContinueWith(_ =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() => SaveSettings());
            });
        }

        private void ResetEq(object obj)
        {
            SelectedPreset = "Flat";
        }

        private void LoadSettings()
        {
            var settings = EqualizerSettingsManager.Load();
            
            // Load band values
            for (int i = 0; i < 10 && i < settings.BandValues.Length; i++)
            {
                Bands[i].Value = settings.BandValues[i];
            }
            
            // Load preamp
            Preamp = settings.Preamp;
            
            // Load preset
            if (!string.IsNullOrEmpty(settings.CurrentPreset))
            {
                _selectedPreset = settings.CurrentPreset;
                OnPropertyChanged(nameof(SelectedPreset));
            }

            // Subscribe to band value changes for auto-save
            foreach (var band in Bands)
            {
                band.ValueChanged += () => SaveSettings();
            }
        }

        private void SaveSettings()
        {
            var settings = new EqualizerSettings
            {
                BandValues = Bands.Select(b => b.Value).ToArray(),
                Preamp = Preamp,
                CurrentPreset = SelectedPreset
            };
            
            EqualizerSettingsManager.Save(settings);
        }
    }
}
