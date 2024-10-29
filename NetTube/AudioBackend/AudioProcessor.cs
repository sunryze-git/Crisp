using System.IO;
using System.Threading.Tasks;
using Concentus;
using Concentus.Oggfile;
using Matroska.Muxer;
using NAudio.Wave;

namespace NetTube.AudioBackend;

public static class AudioProcessor
{
    private const int SampleRate = 48000; // Opus sample rate
    private const int ChannelCount = 2; // Stereo
    private const short BitDepth = 16; // 16-bit samples
    
    // Extracts OggOpus audio from YouTube stream
    public static async Task<MemoryStream> ExtractOggOpusAudio(Stream webmStream)
    {
        var outputStream = new MemoryStream();
        await using var bufferedStream = new BufferedStream(webmStream, 8192);
        MatroskaDemuxer.ExtractOggOpusAudio(bufferedStream, outputStream);
        outputStream.Position = 0;
        return outputStream;
    }
    
    // Converts opus stream directly to PCM and then to WAV
    public static byte[] ConvertOpusToPcm(Stream opusStream)
    {
        // Convert Opus to PCM
        using var decoder = OpusCodecFactory.CreateDecoder(48000, 2);
        var oggIn = new OpusOggReadStream(decoder, opusStream);
        using var wavOut = new MemoryStream();
        
        // Create a RawSourceWaveStream directly from the Opus stream
        using var wavWriter = new WaveFileWriter(wavOut, new WaveFormat(SampleRate, BitDepth, ChannelCount));

        // Buffer for the samples
        while (oggIn.HasNextPacket)
        {
            var packet = oggIn.DecodeNextPacket();
            if (packet is not { Length: > 0 }) continue;

            // Write samples directly to the WAV writer
            wavWriter.WriteSamples(packet, 0, packet.Length);
        }

        wavWriter.Flush();
        return wavOut.ToArray();
    }
}