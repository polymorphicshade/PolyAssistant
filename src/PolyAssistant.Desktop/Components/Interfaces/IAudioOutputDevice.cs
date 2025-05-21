namespace PolyAssistant.Desktop.Components.Interfaces;

public interface IAudioOutputDevice : IAudioDevice, IDisposable, IAsyncDisposable
{
    Task PlayAsync(byte[] wavData, CancellationToken cancellationToken = default);

    void Stop();
}