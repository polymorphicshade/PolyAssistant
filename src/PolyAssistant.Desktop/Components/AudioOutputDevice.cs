using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using PolyAssistant.Desktop.Components.Interfaces;

namespace PolyAssistant.Desktop.Components;

public sealed class AudioOutputDevice(MMDevice rawDevice, ILogger? logger = null) : IAudioOutputDevice
{
    private WaveFileReader? _waveFileReader;
    private WasapiOut? _wasapiOut;
    private MemoryStream? _wavStream;

    public MMDevice RawDevice { get; } = rawDevice;

    public ILogger Logger { get; } = logger ?? NullLogger<AudioOutputDevice>.Instance;

    public string Id => RawDevice.ID;

    public string Name
    {
        get
        {
            try
            {
                return RawDevice.FriendlyName;
            }
            catch (Exception e)
            {
                return "unknown";
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_waveFileReader != null)
        {
            await _waveFileReader.DisposeAsync();
        }

        _wasapiOut?.Dispose();
    }

    public Task PlayAsync(byte[] wavData, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Playing {count} byte(s) of audio on device: {name}", wavData.Length, Name);

        _wavStream = new MemoryStream(wavData);
        _wavStream.Seek(0, SeekOrigin.Begin);

        _waveFileReader = new WaveFileReader(_wavStream);

        _wasapiOut = new WasapiOut(RawDevice, AudioClientShareMode.Shared, true, 200);
        _wasapiOut.Init(_waveFileReader);
        _wasapiOut.Play();

        // return a Task that the caller can optionally await on
        return
            Task
                .Run(() =>
                {
                    while (_wasapiOut.PlaybackState == PlaybackState.Playing)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        Thread.Sleep(50);
                    }
                }, cancellationToken)
                .ContinueWith(_ => _wavStream.Dispose(), cancellationToken);
    }

    public void Stop()
    {
        _wasapiOut?.Stop();
    }

    public void Dispose()
    {
        _wavStream?.Dispose();
        _waveFileReader?.Dispose();
        _wasapiOut?.Dispose();
    }
}