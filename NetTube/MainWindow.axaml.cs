using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using NetTube.Backend;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace NetTube
{
    public partial class MainWindow : Window
    {
        private const string VideoUrl = "https://www.youtube.com/watch?v=ms_8Yfjcymk";
        private readonly AudioPlayer _audioPlayer;
        private readonly ClientManager _clientManager;
        
        public MainWindow()
        {
            InitializeComponent();
            
            _audioPlayer = new AudioPlayer();
            _clientManager = new ClientManager();
        }
        
        // Interactive Methods
        private void PositionSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            if (_audioPlayer.PositionChangeIsProgrammatic) return;
            
            // slider stores percentage through song 0.00 to 1.00
            var newTime = e.NewValue * _audioPlayer.CurrentVideo.Duration!.Value.TotalMilliseconds; // gets new time in ms
            _audioPlayer.MediaPlayer.Time = (long) newTime;
        }

        internal void UpdateSliderPosition(double sliderPos)
        {
            PositionSlider.Value = sliderPos;
        }

        private async void ShuffleButton_OnClick(object? sender, RoutedEventArgs e)
        {
            var video = await _clientManager.GetVideoInfo(VideoUrl);
            _audioPlayer.AddVideo(video);
            _audioPlayer.PlayQueue();
        }

        private void BackSkipButton_OnClick(object? sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void PlayButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (!_audioPlayer.MediaPlayer.IsPlaying) return;
            _audioPlayer.MediaPlayer.SetPause(_audioPlayer.MediaPlayer.IsPlaying);
        }

        private void ForwardSkipButton_OnClick(object? sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RepeatButton_OnClick(object? sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void StatisticsButton_OnClick(object? sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void LibraryButton_OnClick(object? sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BrowseButton_OnClick(object? sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void VolumeSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            _audioPlayer.MediaPlayer.Volume = (int)e.NewValue;
        }
    }
}
