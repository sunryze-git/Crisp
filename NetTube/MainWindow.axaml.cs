using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using NetTube.AudioBackend;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace NetTube
{
    public partial class MainWindow : Window
    {
        const string videoUrl = "https://www.youtube.com/watch?v=n9QRYbS1_DY";

        private readonly AudioPlayer _audioPlayer = new();
        private readonly YoutubeClient _client;
        private Video? _video;

        private bool _sliderChangeIsProgrammatic;
        private List<Video> _queue = [];
        
        public MainWindow()
        {
            InitializeComponent();
            _client = new YoutubeClient();
            
            // Subscribe Content to the Media Player
            _audioPlayer.MediaPlayer.MediaChanged += MediaPlayerOnMediaChanged;
            _audioPlayer.MediaPlayer.PositionChanged += MediaPlayerOnPositionChanged;
        }

        private void MediaPlayerOnPositionChanged(object? sender, MediaPlayerPositionChangedEventArgs e)
        {
            // Update UI with new position information
            Dispatcher.UIThread.Post(() =>
            {
                if (_video is null) return;
                
                var currentTime = TimeSpan.FromMilliseconds(_audioPlayer.MediaPlayer.Time);
                var currentPos = currentTime.TotalSeconds / _video.Duration!.Value.TotalSeconds;
                
                _sliderChangeIsProgrammatic = true;
                PositionSlider.Value = currentPos;
                _sliderChangeIsProgrammatic = false;
                
                SongCurrentTimeLabel.Text = currentTime.ToString(@"m\:ss");
            });
        }

        private void MediaPlayerOnMediaChanged(object? sender, MediaPlayerMediaChangedEventArgs e)
        {
            // Update the UI with new media information
            Dispatcher.UIThread.Post(() =>
            {
                if (_video is null) return;
                
                SongNameLabel.Text = _video.Title;
                SongArtistLabel.Text = _video.Author.ToString();
                SongLengthLabel.Text = _video.Duration!.Value.ToString(@"m\:ss");
            });
        }

        private async Task PlayVideo()
        {
            _video = await _client.Videos.GetAsync(videoUrl);
            var streamManifest = await _client.Videos.Streams.GetManifestAsync(_video.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            _queue.Add(_video);
            
            // TODO: work on queue system
            await _audioPlayer.Play(streamInfo.Url);
        }
        
        // Interactive Methods
        private void PositionSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            if (_sliderChangeIsProgrammatic)
            {
                Console.WriteLine($"Position Slider Changed to: {e.NewValue}");
            }
            else
            {
                // slider stores percentage through song 0.00 to 1.00
                var newTime = e.NewValue * _video.Duration!.Value.TotalMilliseconds; // gets new time in ms
                _audioPlayer.MediaPlayer.Time = (long) newTime;
            }
        }

        private async void ShuffleButton_OnClick(object? sender, RoutedEventArgs e)
        {
            await PlayVideo();
        }

        private void BackSkipButton_OnClick(object? sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void PlayButton_OnClick(object? sender, RoutedEventArgs e)
        {
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
    }
}
