using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using NAudio.Wave;

using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace NostalgiaLogger;

public class NostalgiaLogger : Logger
{
    private bool playbackActive;

    private Mp3FileReader audioReader;
    private WaveOutEvent outputDevice;

    private ResourceManager ResourceManager { get; } = new("NostalgiaLogger.Resources", typeof(NostalgiaLogger).Assembly);

    public override void Initialize(IEventSource eventSource)
    {
        eventSource.BuildStarted += OnBuildStarted;
        eventSource.BuildFinished += OnBuildFinished;
    }

    private void OnBuildStarted(object sender, BuildStartedEventArgs e)
    {
        Thread playbackThread = new(StartPlayback);
        playbackThread.Start();
    }

    private void OnBuildFinished(object sender, BuildFinishedEventArgs e)
    {
        StopPlayback();
    }

    private void StartPlayback()
    {
        audioReader = new Mp3FileReader(new MemoryStream((byte[])ResourceManager.GetObject("HardDriveSound")));
        outputDevice = new WaveOutEvent();

        playbackActive = true;

        outputDevice.Init(audioReader);

        while (playbackActive)
        {
            outputDevice.Play();
            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(1000);
            }
        }
    }

    public void StopPlayback()
    {
        playbackActive = false;

        outputDevice?.Stop();

        outputDevice?.Dispose();
        audioReader?.Dispose();
    }
}
