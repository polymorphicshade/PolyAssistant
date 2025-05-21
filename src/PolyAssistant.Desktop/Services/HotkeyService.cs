using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PolyAssistant.Desktop.Services.Interfaces;
using SharpHook;
using SharpHook.Data;

namespace PolyAssistant.Desktop.Services;

public sealed class HotkeyService : IHotkeyService
{
    private readonly TaskPoolGlobalHook _hook = new();
    private bool _flag;

    public HotkeyService(ILogger<HotkeyService> logger, IConfiguration configuration)
    {
        var hotKeyStr = configuration["HotKey"] ?? string.Empty;
        if (!hotKeyStr.Contains("Vc"))
        {
            hotKeyStr = $"Vc{hotKeyStr}";
        }

        if (!Enum.TryParse(typeof(KeyCode), hotKeyStr, out var result))
        {
            Hotkey = KeyCode.VcRightControl;
        }
        else
        {
            Hotkey = (KeyCode)result;
        }

        logger.LogInformation("Registered hotkey: {key}", Hotkey);

        _hook.KeyPressed += OnKeyPressed;
        _hook.KeyReleased += OnKeyReleased;

        // asynchronously listen for keyboard events
        _ = _hook.RunAsync();
    }

    public KeyCode Hotkey { get; }

    public void Dispose()
    {
        _hook.Dispose();
    }

    public event EventHandler<KeyboardHookEventArgs> HotKeyPressed = delegate { };

    public event EventHandler<KeyboardHookEventArgs> HotKeyReleased = delegate { };

    public void Suspend()
    {
        _hook.Stop();
    }

    public void Resume()
    {
        _ = _hook.RunAsync();
    }

    private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        if (_flag)
        {
            return;
        }

        var keyCode = e.Data.KeyCode;

        if (keyCode == Hotkey)
        {
            e.SuppressEvent = true;

            _flag = true;

            HotKeyPressed(this, e);
        }
    }

    private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        if (!_flag)
        {
            return;
        }

        var keyCode = e.Data.KeyCode;

        if (keyCode == Hotkey)
        {
            e.SuppressEvent = true;

            _flag = false;

            HotKeyReleased(this, e);
        }
    }
}