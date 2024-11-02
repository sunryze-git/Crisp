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
using NetTube.AudioBackend;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace NetTube
{
    public partial class MainWindow : Window
    {
        private const string VideoUrl = "https://www.youtube.com/watch?v=ms_8Yfjcymk";

        private readonly AudioPlayer _audioPlayer = new();
        private readonly YoutubeClient _youtubeClient;
        private Video? _currentVideo;

        private bool _sliderChangeIsProgrammatic;
        private Queue<VideoInformation> _queue = [];

        private struct VideoInformation
        {
            public string Title;
            public string Artist;
            public string? Album;
            public TimeSpan? Duration;
            public string StreamUrl;
            public string? ThumbnailUrl;
        }
        
        public MainWindow()
        {
            InitializeComponent();
            _youtubeClient = new YoutubeClient();
            
            // Subscribe Content to the Media Player
            _audioPlayer.MediaPlayer.MediaChanged += MediaPlayerOnMediaChanged;
            _audioPlayer.MediaPlayer.PositionChanged += MediaPlayerOnPositionChanged;
            _audioPlayer.MediaPlayer.EndReached += MediaPlayerOnEndReached;
            _audioPlayer.MediaPlayer.Volume = 100;
        }

        private void MediaPlayerOnEndReached(object? sender, EventArgs e)
        {
            if (_queue.Count == 0) return;
            PlayQueue(); // if queue isn't empty, play next one
        }

        // Dequeues the next song, gets the stream URL, and plays it with the media player
        private async void PlayQueue()
        {
            if (_queue.Count == 0) return;
            
            var nextVideo = _queue.Dequeue();
            await _audioPlayer.Play(nextVideo.StreamUrl);
        }

        // Gets an audio stream URL from a YouTube Video URL, also sets current video
        private async Task<string> GetAudioStreamUrl(string url)
        {
            _currentVideo = await _youtubeClient.Videos.GetAsync(url);
            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(_currentVideo.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            return streamInfo.Url;
        }
        
        // Adds a YouTube URL to the Queue
        private async Task AddToQueue(string url)
        {
            var streamUrl = await GetAudioStreamUrl(url);
            if (_currentVideo is null) return;

            // creates new video object that makes it easier to get what we need
            var playableVideo = new VideoInformation
            {
                Title = _currentVideo.Title,
                Artist = _currentVideo.Author.ChannelTitle,
                Duration = _currentVideo.Duration,
                StreamUrl = streamUrl,
                ThumbnailUrl = _currentVideo.Thumbnails.FirstOrDefault()?.Url
            };
            _queue.Enqueue(playableVideo);
        }

        private void MediaPlayerOnPositionChanged(object? sender, MediaPlayerPositionChangedEventArgs e)
        {
            // Update UI with new position information
            Dispatcher.UIThread.Post(() =>
            {
                if (_currentVideo is null) return;
                
                var currentTime = TimeSpan.FromMilliseconds(_audioPlayer.MediaPlayer.Time);
                var currentPos = currentTime.TotalSeconds / _currentVideo.Duration!.Value.TotalSeconds;
                
                _sliderChangeIsProgrammatic = true;
                PositionSlider.Value = currentPos;
                _sliderChangeIsProgrammatic = false;
                
                SongCurrentTimeLabel.Text = currentTime.ToString(@"m\:ss");
            });
        }

        private static async Task<Bitmap?> LoadUrlImage(string url)
        {
            using HttpClient client = new();
            try
            {
                Console.WriteLine($"Loading Thumbnail: {url}");
                byte[] data = await client.GetByteArrayAsync(url);

                await using var imageStream = new MemoryStream(data);
                var preCrop = new Bitmap(imageStream);

                return preCrop;
            }
            catch (HttpRequestException e)
            {
                Console.Error.WriteLine(e);
                return null;
            }
        }

        private async void MediaPlayerOnMediaChanged(object? sender, MediaPlayerMediaChangedEventArgs e)
        {
            if (_currentVideo is null) return;
            
            var thumbnail = await LoadUrlImage(_currentVideo.Thumbnails.GetWithHighestResolution().Url);
            
            // Update the UI with new media information
            Dispatcher.UIThread.Post(() =>
            {
                
                SongNameLabel.Text = _currentVideo.Title;
                SongArtistLabel.Text = _currentVideo.Author.ToString();
                SongLengthLabel.Text = _currentVideo.Duration!.Value.ToString(@"m\:ss");
                ThumbnailImage.Source = thumbnail;
            });
        }
        
        // Interactive Methods
        private void PositionSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            if (_currentVideo is null) return;
            if (_sliderChangeIsProgrammatic) return;
            
            // slider stores percentage through song 0.00 to 1.00
            var newTime = e.NewValue * _currentVideo.Duration!.Value.TotalMilliseconds; // gets new time in ms
            _audioPlayer.MediaPlayer.Time = (long) newTime;
        }

        private async void ShuffleButton_OnClick(object? sender, RoutedEventArgs e)
        {
            await AddToQueue(VideoUrl);
            PlayQueue();
        }

        private void BackSkipButton_OnClick(object? sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void PlayButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (_currentVideo is null) return;
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
