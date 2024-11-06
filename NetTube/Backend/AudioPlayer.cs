using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using LibVLCSharp.Shared;

namespace NetTube.Backend;

public class AudioPlayer
{
    /// <summary>
    /// Primary entry point to control the VLC Player.
    /// You shouldn't need to control the Media part; that's handled by this.
    /// </summary>
    public readonly MediaPlayer MediaPlayer;

    public bool PositionChangeIsProgrammatic;

    internal MediaManager.VideoInformation CurrentVideo;
    private static readonly Queue<MediaManager.VideoInformation> Queue = [];
    private readonly LibVLC _libVlc;
    
    /// <summary>
    /// Creates a new instance of the Audio Player.
    /// </summary>
    public AudioPlayer()
    {
        Core.Initialize("");
        _libVlc = new LibVLC();
        MediaPlayer = new MediaPlayer(_libVlc);
        
        // Default Volume
        // TODO: when config system, set this to the last set volume
        MediaPlayer.Volume = 100;
        
        // Subscribe Events
        MediaPlayer.EndReached += MediaPlayer_OnEndReached;
        MediaPlayer.PositionChanged += MediaPlayer_OnPlayback;
        MediaPlayer.MediaChanged += MediaPlayer_OnMediaChange;
        
        Console.WriteLine("Audio Player Initialized.");
    }

    /// <summary>
    /// Begins playback of the current queue.
    /// </summary>
    internal async void PlayQueue()
    {
        if (Queue.Count == 0) return;
        
        var nextVideo = Queue.Dequeue();
        CurrentVideo = nextVideo;
        await Play(nextVideo);
    }

    /// <summary>
    /// Stops and empties the queue
    /// </summary>
    internal void StopQueue()
    {
        MediaPlayer.Stop();
        Queue.Clear();
    }
    
    /// <summary>
    /// Gets the next VideoInformation object in the queue.
    /// </summary>
    /// <returns></returns>
    internal static MediaManager.VideoInformation? GetNext()
    {
        if (Queue.Count == 0) return null;
        
        return Queue.Dequeue();
    }
    
    /// <summary>
    /// Adds a new VideoInformation object to the queue.
    /// </summary>
    /// <param name="video"></param>
    internal void AddVideo(MediaManager.VideoInformation video)
    {
        Queue.Enqueue(video);
    }

    /// <summary>
    /// Plays a given YouTube URL file in the background. You will need to block on
    /// the main thread in order to let this play, otherwise it will not.
    /// </summary> 
    /// <param name="url"></param>
    /// <param name="video"></param>
    private async Task Play(MediaManager.VideoInformation video)
    {
        var url = video.StreamUrl;
        var media = new Media(_libVlc, new Uri(url));
        MediaPlayer.Media = media;
        
        await Task.Run(() =>
        {
            Console.WriteLine($"Playing {url}");
            MediaPlayer.Play();

            while (MediaPlayer.IsPlaying)
            {
                Task.Delay(100).Wait();
            }
        });
    }

    /// <summary>
    /// Built in event handler to move onto the next song in the queue when the current playback ends.
    /// If no new songs next, we just stop.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MediaPlayer_OnEndReached(object? sender, EventArgs e)
    { 
        // logic here is that once started, it will go until the count is 0
        PlayQueue();
    }

    /// <summary>
    /// Called during playback, updates the UI elements.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MediaPlayer_OnPlayback(object? sender, MediaPlayerPositionChangedEventArgs e)
    {
        // Update UI
        Dispatcher.UIThread.Post(() =>
        {
            var currentTime = TimeSpan.FromMilliseconds(MediaPlayer.Time);
            var currentSliderPos = currentTime.TotalMilliseconds / MediaPlayer.Media!.Duration;

            PositionChangeIsProgrammatic = true;
            // TODO: update slider idk how
            PositionChangeIsProgrammatic = false;
            
            // TODO: update current time label, set to @"m\:ss" format
        });
    }

    /// <summary>
    /// Called when the media object changes, basically called on a new playback.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MediaPlayer_OnMediaChange(object? sender, MediaPlayerMediaChangedEventArgs e)
    {
        var thumbnail = await LoadUrlImage(CurrentVideo.ThumbnailUrl!);
        
        // Update UI
        Dispatcher.UIThread.Post(() =>
        {
            // update UI elements or do the binding here
            // SongNameLabel.Text = _currentVideo.Title;
            // SongArtistLabel.Text = _currentVideo.Author.ToString();
            // SongLengthLabel.Text = _currentVideo.Duration!.Value.ToString(@"m\:ss");
            // ThumbnailImage.Source = thumbnail;
        });
    }


    /// <summary>
    /// Loads a Bitmap from a thumbnail URL.
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
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
}