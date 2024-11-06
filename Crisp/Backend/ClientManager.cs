using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace NetTube.Backend;

/// <summary>
/// Provides a method to get a VideoInformation object from a video URL.
/// </summary>

public class ClientManager
{
    private readonly YoutubeClient _client;
    
    internal ClientManager()
    {
        _client = new YoutubeClient();
    }

    internal async Task<MediaManager.VideoInformation> GetVideoInfo(string url)
    {
        var video = await _client.Videos.GetAsync(url);
        var streamManifest = await _client.Videos.Streams.GetManifestAsync(url);
        var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

        var vi = new MediaManager.VideoInformation
        {
            Title = video.Title,
            Artist = video.Author.ChannelTitle,
            Duration = video.Duration,
            StreamUrl = streamInfo.Url,
            ThumbnailUrl = video.Thumbnails[0].Url
        };

        return vi;
    }
}