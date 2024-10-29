using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NetTube.AudioBackend;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace NetTube
{
    public partial class MainWindow : Window
    {
        const string videoUrl = "https://www.youtube.com/watch?v=n9QRYbS1_DY";

        private readonly YoutubeClient _client;
        private Video? _video;
        
        public MainWindow()
        {
            InitializeComponent();
            this._client = new YoutubeClient();
            AudioPlayer.Initialize();
            AudioPlayer.OnContextTick += UpdateSongPosition;
        }

        private void UpdateSongPosition()
        {
        }

        public async void PlayButtonPressed(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Button has been pressed.");
            StartMusic(videoUrl);
        }

        private async void StartMusic(string url)
        {
            this._video = await _client.Videos.GetAsync(url);
            var streamManifest = await _client.Videos.Streams.GetManifestAsync(_video.Id);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            await using var stream = await _client.Videos.Streams.GetAsync(streamInfo);

            var opusStream = await AudioProcessor.ExtractOggOpusAudio(stream);
            var pcmData = AudioProcessor.ConvertOpusToPcm(opusStream);
            
            AudioPlayer.Play(pcmData);
            AudioPlayer.WaitForPlayback();
        }
    }
}
