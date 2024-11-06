using System;
using System.Linq;
using Avalonia.Threading;

namespace NetTube.Backend;
/// <summary>
/// Provides event handlers for the AudioPlayer.
/// </summary>
public class MediaManager
{
    public struct VideoInformation
    {
        public string Title;
        public string Artist;
        public string? Album;
        public TimeSpan? Duration;
        public string StreamUrl;
        public string? ThumbnailUrl;
    }
}