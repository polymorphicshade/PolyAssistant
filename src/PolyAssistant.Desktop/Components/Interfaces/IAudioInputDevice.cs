using NAudio.Wave;

namespace PolyAssistant.Desktop.Components.Interfaces;

public interface IAudioInputDevice : IAudioDevice, IDisposable
{
    bool IsListening { get; }

    bool IsRecording { get; }

    bool IsPolling { get; }

    event EventHandler<WaveInEventArgs> DataAvailable;

    event EventHandler<byte[]> DataPolled;

    void StartListening();

    byte[] StopListening();

    void StartRecording(bool throwOnNotListening = false);

    byte[] StopRecording();

    Task<byte[]> RecordAsync(TimeSpan timeSpan, bool throwOnNotListening = false, CancellationToken cancellationToken = default);

    Task StartPolling(TimeSpan interval, bool throwOnNotListening = false, CancellationToken cancellationToken = default);

    byte[] StopPolling();

    Task<byte[]> PollAsync(TimeSpan interval, TimeSpan timeSpan, bool throwOnNotListening = false, CancellationToken cancellationToken = default);

    void StopAll();
}