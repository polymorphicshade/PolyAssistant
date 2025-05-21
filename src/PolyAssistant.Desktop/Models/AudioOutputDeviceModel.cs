using CommunityToolkit.Mvvm.ComponentModel;
using PolyAssistant.Desktop.Components;
using PolyAssistant.Desktop.Components.Interfaces;

namespace PolyAssistant.Desktop.Models;

public partial class AudioOutputDeviceModel(IAudioOutputDevice device) : Model
{
    [ObservableProperty] private bool _isSelected;

    public IAudioOutputDevice Device { get; } = device;
}