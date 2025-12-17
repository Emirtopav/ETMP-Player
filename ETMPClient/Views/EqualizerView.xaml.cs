using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETMPClient.Views
{
    public partial class EqualizerView : UserControl
    {
        public EqualizerView()
        {
            InitializeComponent();
        }

        // Mouse wheel support for fine-tuning
        private void Slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is Slider slider)
            {
                // Fine adjustment: 0.5 dB per wheel notch
                double delta = e.Delta > 0 ? 0.5 : -0.5;
                double newValue = slider.Value + delta;
                
                // Clamp to min/max
                if (newValue >= slider.Minimum && newValue <= slider.Maximum)
                {
                    slider.Value = newValue;
                }
                
                e.Handled = true;
            }
        }
    }
}
