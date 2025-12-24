using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ETMPClient.Models
{
    public class ChannelControl : INotifyPropertyChanged
    {
        private int _channelNumber;
        private int _volume = 100;
        private bool _isMuted;
        private bool _isSolo;
        private bool _isActive;
        private string _instrumentName = "Unknown";

        public int ChannelNumber
        {
            get => _channelNumber;
            set { _channelNumber = value; OnPropertyChanged(); }
        }

        public int Volume
        {
            get => _volume;
            set { _volume = value; OnPropertyChanged(); }
        }

        public bool IsMuted
        {
            get => _isMuted;
            set { _isMuted = value; OnPropertyChanged(); }
        }

        public bool IsSolo
        {
            get => _isSolo;
            set { _isSolo = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        public string InstrumentName
        {
            get => _instrumentName;
            set { _instrumentName = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
