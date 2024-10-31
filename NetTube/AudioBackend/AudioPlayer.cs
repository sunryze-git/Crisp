using System;
using System.Threading.Tasks;
using LibVLCSharp.Shared;

namespace NetTube.AudioBackend;

public class AudioPlayer
{
    /// <summary>
    /// Primary entry point to control the VLC Player.
    /// You shouldn't need to control the Media part- thats handled by this.
    /// </summary>
    public readonly MediaPlayer MediaPlayer;
    
    private readonly LibVLC _libVlc;
    
    /// <summary>
    /// Creates a new instance of the Audio Player.
    /// </summary>
    public AudioPlayer()
    {
        Core.Initialize("");
        _libVlc = new LibVLC();
        MediaPlayer = new MediaPlayer(_libVlc);
        
        Console.WriteLine("Audio Player Initialized.");
    }

    /// <summary>
    /// Plays a given YouTube URL file in the background. You will need to block on
    /// the main thread in order to let this play, otherwise it will not.
    /// </summary> 
    /// <param name="url"></param>
    public async Task Play(string url)
    {
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
}