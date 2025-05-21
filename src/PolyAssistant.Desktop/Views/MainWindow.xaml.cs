using System.Windows;
using System.Windows.Input;
using PolyAssistant.Desktop.Models;
using PolyAssistant.Desktop.Services;
using PolyAssistant.Desktop.ViewModels;
using SharpHook;

namespace PolyAssistant.Desktop.Views;

public partial class MainWindow
{
    private readonly HotkeyService _hotkeyService;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = Application.Current.GetOrCreateInstance<MainViewModel>();

        _hotkeyService = Application.Current.GetOrCreateInstance<HotkeyService>();

        _hotkeyService.HotKeyPressed += OnHotKeyPressed;
        _hotkeyService.HotKeyReleased += OnHotKeyReleased;

        Hide();

        Application.Current.Exit += OnApplicationExit;
    }

    public MainViewModel ViewModel => (MainViewModel)DataContext;

    // UI events

    private async void OnTrayAgentMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (sender is not FrameworkElement element)
            {
                return;
            }

            if (element.DataContext is not AgentModel model)
            {
                return;
            }

            await ViewModel.SelectAgentCommand.ExecuteAsync(model);
        }
        catch
        {
            // ignored
        }
    }

    private void OnTrayInputDeviceMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (element.DataContext is not AudioInputDeviceModel model)
        {
            return;
        }

        ViewModel.SelectInputDeviceCommand.Execute(model);
    }

    private void OnTrayOutputDeviceMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (element.DataContext is not AudioOutputDeviceModel model)
        {
            return;
        }

        ViewModel.SelectOutputDeviceCommand.Execute(model);
    }

    private void OnTrayMenuExitClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    // hotkey events

    private void OnHotKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        Dispatcher.InvokeAsync(() => ViewModel.StartRecordingCommand.Execute(null));
    }

    private void OnHotKeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        Dispatcher.InvokeAsync(() =>
        {
            if (ViewModel.StopRecordingCommand.IsRunning)
            {
                return;
            }

            _hotkeyService.Suspend();

            _ = ViewModel.StopRecordingCommand.ExecuteAsync(null).ContinueWith(x => _hotkeyService.Resume());
        });
    }

    // application events

    private void OnApplicationExit(object sender, ExitEventArgs e)
    {
        _hotkeyService.HotKeyPressed -= OnHotKeyPressed;
        _hotkeyService.HotKeyReleased -= OnHotKeyReleased;

        _hotkeyService.Dispose();

        ViewModel.Dispose();
    }
}