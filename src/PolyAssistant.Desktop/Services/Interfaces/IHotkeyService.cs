using SharpHook;
using SharpHook.Data;

namespace PolyAssistant.Desktop.Services.Interfaces;

public interface IHotkeyService : IDisposable
{
    KeyCode Hotkey { get; }

    event EventHandler<KeyboardHookEventArgs> HotKeyPressed;

    event EventHandler<KeyboardHookEventArgs> HotKeyReleased;

    void Suspend();

    void Resume();
}