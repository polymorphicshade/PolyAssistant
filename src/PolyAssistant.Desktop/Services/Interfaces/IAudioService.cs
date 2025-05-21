using NAudio.CoreAudioApi;
using PolyAssistant.Desktop.Components.Interfaces;

namespace PolyAssistant.Desktop.Services.Interfaces;

public interface IAudioService
{
    IEnumerable<IAudioInputDevice> GetInputDevices();

    IEnumerable<IAudioOutputDevice> GetOutputDevices();

    IAudioInputDevice GetDefaultInputDevice(Role role = Role.Communications);

    IAudioOutputDevice GetDefaultOutputDevice(Role role = Role.Communications);
}