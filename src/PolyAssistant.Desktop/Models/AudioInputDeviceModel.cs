using CommunityToolkit.Mvvm.ComponentModel;
using PolyAssistant.Desktop.Components;
using PolyAssistant.Desktop.Components.Interfaces;

namespace PolyAssistant.Desktop.Models;

public partial class AudioInputDeviceModel(IAudioInputDevice device) : Model
{
    [ObservableProperty] private bool _isSelected;

    public IAudioInputDevice Device { get; } = device;
}