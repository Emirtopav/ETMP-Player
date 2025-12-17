using ETMPClient.ViewModels;

namespace ETMPClient.Models
{
    public class VisualBarViewModel : ViewModelBase
    {
        private double _value;
        public double Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
