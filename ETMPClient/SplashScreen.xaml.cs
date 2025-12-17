using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ETMPClient
{
    public partial class SplashScreen : Window
    {
        private DispatcherTimer _progressTimer = new();
        private double _progress = 0;

        public SplashScreen()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Start fade-in animation
            var fadeIn = (Storyboard)FindResource("FadeInStoryboard");
            fadeIn.Begin();

            // Animate progress bar
            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            _progressTimer.Tick += ProgressTimer_Tick;
            _progressTimer.Start();
        }

        private void ProgressTimer_Tick(object? sender, EventArgs e)
        {
            _progress += 2;
            ProgressBar.Width = Math.Min(_progress, 200);

            if (_progress >= 200)
            {
                _progressTimer.Stop();
                
                // Wait a bit then fade out
                var closeTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(300)
                };
                closeTimer.Tick += (s, args) =>
                {
                    closeTimer.Stop();
                    var fadeOut = (Storyboard)FindResource("FadeOutStoryboard");
                    fadeOut.Begin();
                };
                closeTimer.Start();
            }
        }

        private void FadeOut_Completed(object sender, EventArgs e)
        {
            Close();
        }
    }
}

