using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ETMPClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            InitializeComponent();
            
            // Add keyboard shortcut handler
            this.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Don't handle shortcuts when typing in TextBox
            if (e.OriginalSource is TextBox)
                return;

            var viewModel = DataContext as ViewModels.MainViewModel;
            var playerViewModel = viewModel?.PlayerView as ViewModels.PlayerViewModel;
            
            if (playerViewModel == null) return;

            // Check for Ctrl modifier
            bool ctrlPressed = Keyboard.Modifiers == ModifierKeys.Control;

            switch (e.Key)
            {
                case Key.Space:
                    // Play/Pause
                    playerViewModel.TogglePlayer.Execute(null);
                    e.Handled = true;
                    break;

                case Key.Left:
                    if (ctrlPressed)
                    {
                        // Previous track
                        playerViewModel.PlayBackward.Execute(null);
                    }
                    else
                    {
                        // Seek backward 10s
                        playerViewModel.SongProgress = Math.Max(0, playerViewModel.SongProgress - 10);
                    }
                    e.Handled = true;
                    break;

                case Key.Right:
                    if (ctrlPressed)
                    {
                        // Next track
                        playerViewModel.PlayForward.Execute(null);
                    }
                    else
                    {
                        // Seek forward 10s
                        playerViewModel.SongProgress = Math.Min(playerViewModel.SongDuration, 
                                                                playerViewModel.SongProgress + 10);
                    }
                    e.Handled = true;
                    break;

                case Key.Up:
                    // Volume up 5%
                    playerViewModel.Volume = Math.Min(100, playerViewModel.Volume + 5);
                    e.Handled = true;
                    break;

                case Key.Down:
                    // Volume down 5%
                    playerViewModel.Volume = Math.Max(0, playerViewModel.Volume - 5);
                    e.Handled = true;
                    break;

                // Media Keys
                case Key.MediaPlayPause:
                    playerViewModel.TogglePlayer.Execute(null);
                    e.Handled = true;
                    break;

                case Key.MediaNextTrack:
                    playerViewModel.PlayForward.Execute(null);
                    e.Handled = true;
                    break;

                case Key.MediaPreviousTrack:
                    playerViewModel.PlayBackward.Execute(null);
                    e.Handled = true;
                    break;

                case Key.MediaStop:
                    // Stop playback (pause)
                    if (playerViewModel.IsPlaying)
                        playerViewModel.TogglePlayer.Execute(null);
                    e.Handled = true;
                    break;
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState != WindowState.Maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
                if (sender is Button button)
                    button.Content = "❐";
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                if (sender is Button button)
                    button.Content = "▢";
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        public void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.BorderThickness = new Thickness(6);
            }
            else
            {
                this.BorderThickness = new Thickness(0);
            }
        }
    }
}

