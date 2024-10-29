using System;
using System.Threading;
using Microsoft.VisualBasic;
using MiniAudioEx;

namespace NetTube.AudioBackend;

public static class AudioPlayer
{
    private const uint SampleRate = 48000;
    private const uint Channels = 2;

    private static AudioSource? _source;
    private static AudioClip? _clip;

    public delegate void ContextTick();
    public static event ContextTick? OnContextTick;
    
    public static ulong Position { get; private set; }

    public static void Initialize()
    {
        Console.CancelKeyPress += OnCancel;
        AudioContext.Initialize(SampleRate, Channels);
        Console.WriteLine("Audio player initialized.");
    }
    
    public static void Play(byte[] audioStream)
    {
        Console.WriteLine($"Loaded {audioStream.Length} bytes into memory.");
        _source = new AudioSource();
        _clip = new AudioClip(audioStream);
        _source.Play(_clip);
    }

    public static void WaitForPlayback()
    {
        while (_source is { IsPlaying: true })
        {
            AudioContext.Update();
            Thread.Sleep(10);

            Position = _source.Cursor;
            OnContextTick?.Invoke();
        }
    }

    private static void OnCancel(object sender, ConsoleCancelEventArgs e)
    {
        _source?.Stop();
        AudioContext.Deinitialize();
    }
}