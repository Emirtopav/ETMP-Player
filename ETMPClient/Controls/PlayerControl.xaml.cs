using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ETMPClient.ViewModels;

namespace ETMPClient.Controls
{
    /// <summary>
    /// Interaction logic for PlayerComponent.xaml
    /// </summary>
    public partial class PlayerControl : UserControl
    {
        private string _lastSongName = string.Empty;
        private string _lastArtistName = string.Empty;

        public PlayerControl()
        {
            InitializeComponent();
            DataContextChanged += PlayerControl_DataContextChanged;
        }

        private void PlayerControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Unsubscribe from old ViewModel
            if (e.OldValue is INotifyPropertyChanged oldViewModel)
            {
                oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            // Subscribe to new ViewModel
            if (e.NewValue is INotifyPropertyChanged newViewModel)
            {
                newViewModel.PropertyChanged += ViewModel_PropertyChanged;
                
                // Initialize last values
                if (e.NewValue is PlayerViewModel vm)
                {
                    _lastSongName = vm.SongName!;
                    _lastArtistName = vm.ArtistName!;
                }
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not PlayerViewModel vm) return;

            // Animate song title change
            if (e.PropertyName == nameof(PlayerViewModel.SongName) && vm.SongName != _lastSongName)
            {
                _lastSongName = vm.SongName!;
                AnimateFadeIn(SongTitleText);
            }

            // Animate artist name change
            if (e.PropertyName == nameof(PlayerViewModel.ArtistName) && vm.ArtistName != _lastArtistName)
            {
                _lastArtistName = vm.ArtistName!;
                AnimateFadeIn(ArtistNameText);
            }
        }

        private void AnimateFadeIn(UIElement element)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.3,
                Duration = TimeSpan.FromMilliseconds(120),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeIn = new DoubleAnimation
            {
                From = 0.3,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(180),
                BeginTime = TimeSpan.FromMilliseconds(120),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            var storyboard = new Storyboard();
            storyboard.Children.Add(fadeOut);
            storyboard.Children.Add(fadeIn);

            Storyboard.SetTarget(fadeOut, element);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTarget(fadeIn, element);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));

            storyboard.Begin();
        }
    }
}

