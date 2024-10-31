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
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace NetTube
{
    public partial class MainWindow : Window
    {
        const string videoUrl = "https://music.youtube.com/watch?v=Nn-qUWyVBqw";

        private readonly AudioPlayer _audioPlayer = new();
        private readonly YoutubeClient _client;
        private Video? _video;

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
            _client = new YoutubeClient();
            
            // Subscribe Content to the Media Player
            _audioPlayer.MediaPlayer.MediaChanged += MediaPlayerOnMediaChanged;
            _audioPlayer.MediaPlayer.PositionChanged += MediaPlayerOnPositionChanged;
        }

        private static void PlayQueue()
        {
            // 
        }
        
        private async Task AddToQueue(string url)
        {
            _video = await _client.Videos.GetAsync(url);
            var streamManifest = await _client.Videos.Streams.GetManifestAsync(_video.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            // creates new video object that makes it easier to get what we need
            var playableVideo = new VideoInformation
            {
                Title = _video.Title,
                Artist = _video.Author.ChannelTitle,
                Duration = _video.Duration,
                StreamUrl = streamInfo.Url,
                ThumbnailUrl = _video.Thumbnails.FirstOrDefault()?.Url
            };
            _queue.Enqueue(playableVideo);
            
            // TODO: work on queue system
            await _audioPlayer.Play(streamInfo.Url);
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

        private static async Task<Bitmap?> LoadUrlImage(string url)
        {
            using HttpClient client = new();
            try
            {
                Console.WriteLine($"Loading Thumbnail: {url}");
                byte[] data = await client.GetByteArrayAsync(url);

                await using var imageStream = new MemoryStream(data);
                var preCrop = new Bitmap(imageStream);

                var cropRect = new Rect(
                    (preCrop.PixelSize.Width) - 90d,
                    (preCrop.PixelSize.Height) - 90d,
                    180,
                    180
                );
                
                return preCrop.Clone(cropRect, preCrop.PixelFormat);
            }
            catch (HttpRequestException e)
            {
                Console.Error.WriteLine(e);
                return null;
            }
        }

        private async void MediaPlayerOnMediaChanged(object? sender, MediaPlayerMediaChangedEventArgs e)
        {
            if (_video is null) return;
            foreach (var thumb in _video.Thumbnails)
            {
                Console.WriteLine(thumb.Url);
            }
            
            var thumbnail = await LoadUrlImage(_video.Thumbnails.Last().Url);
            
            // Update the UI with new media information
            Dispatcher.UIThread.Post(() =>
            {
                
                SongNameLabel.Text = _video.Title;
                SongArtistLabel.Text = _video.Author.ToString();
                SongLengthLabel.Text = _video.Duration!.Value.ToString(@"m\:ss");
                ThumbnailImage.Source = thumbnail;
            });
        }
        
        // Interactive Methods
        private void PositionSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            if (_video is null) return;
            
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
            await AddToQueue(videoUrl);
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
