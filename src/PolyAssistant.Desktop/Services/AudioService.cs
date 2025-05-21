using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;
using PolyAssistant.Desktop.Components;
using PolyAssistant.Desktop.Components.Interfaces;
using PolyAssistant.Desktop.Services.Interfaces;

namespace PolyAssistant.Desktop.Services;

public sealed class AudioService(ILogger<AudioService> logger) : IAudioService
{
    public IEnumerable<IAudioInputDevice> GetInputDevices()
    {
        return
            new MMDeviceEnumerator()
                .EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
                .Select(device => new AudioInputDevice(device, logger));
    }

    public IEnumerable<IAudioOutputDevice> GetOutputDevices()
    {
        return
            new MMDeviceEnumerator()
                .EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                .Select(device => new AudioOutputDevice(device, logger));
    }

    public IAudioInputDevice GetDefaultInputDevice(Role role = Role.Communications)
    {
        var enumerator = new MMDeviceEnumerator();

        var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, role);

        return new AudioInputDevice(device, logger);
    }

    public IAudioOutputDevice GetDefaultOutputDevice(Role role = Role.Communications)
    {
        var enumerator = new MMDeviceEnumerator();

        var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, role);

        return new AudioOutputDevice(device, logger);
    }
}