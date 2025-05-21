using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;

namespace PolyAssistant.Desktop.Components.Interfaces;

public interface IAudioDevice
{
    MMDevice RawDevice { get; }

    ILogger Logger { get; }

    public string Id { get; }

    public string Name { get; }
}