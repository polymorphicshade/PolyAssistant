using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PolyAssistant.Core.Services.Interfaces;
using PolyAssistant.Desktop.Agents.Interfaces;
using PolyAssistant.Desktop.Components;
using PolyAssistant.Desktop.Models;
using PolyAssistant.Desktop.Services.Interfaces;

// ReSharper disable PartialTypeWithSinglePart

namespace PolyAssistant.Desktop.ViewModels;

public partial class MainViewModel : ViewModel, IDisposable
{
    private readonly ILogger<MainViewModel> _logger = NullLogger<MainViewModel>.Instance;

    [ObservableProperty] private Mode _selectedMode = Mode.Command;
    [ObservableProperty] private ObservableCollection<AgentModel> _agents = [];
    [ObservableProperty] private ObservableCollection<AudioInputDeviceModel> _inputDevices = [];
    [ObservableProperty] private ObservableCollection<AudioOutputDeviceModel> _outputDevices = [];

    public MainViewModel()
    {
        // required for design-time development
    }

    [ActivatorUtilitiesConstructor]
    public MainViewModel(ILogger<MainViewModel> logger, IAudioService audioService, IAgentService agentService, IRemoteFilesClientService remoteFilesClientService, IRemoteChatClientService remoteChatClientService, IRemoteTextClientService remoteTextClientService, IRemoteVoiceClientService remoteVoiceClientService)
    {
        _logger = logger;

        AudioService = audioService;
        AgentService = agentService;

        RemoteFilesClientService = remoteFilesClientService;
        RemoteChatClientService = remoteChatClientService;
        RemoteTextClientService = remoteTextClientService;
        RemoteVoiceClientService = remoteVoiceClientService;

        RefreshDevices();
        _ = RefreshAgentsAsync();
    }

    protected IAudioService AudioService { get; } = null!;

    protected IAgentService AgentService { get; } = null!;

    protected IRemoteChatClientService RemoteChatClientService { get; } = null!;

    protected IRemoteTextClientService RemoteTextClientService { get; } = null!;

    protected IRemoteVoiceClientService RemoteVoiceClientService { get; } = null!;

    protected IRemoteFilesClientService RemoteFilesClientService { get; } = null!;

    public AgentModel? SelectedAgent => Agents.FirstOrDefault(x => x.IsSelected);

    public AudioInputDeviceModel? SelectedInputDevice => InputDevices.FirstOrDefault(x => x.IsSelected);

    public AudioOutputDeviceModel? SelectedOutputDevice => OutputDevices.FirstOrDefault(x => x.IsSelected);

    public void Dispose()
    {
        foreach (var device in
                 InputDevices
                     .Select(x => x.Device)
                     .Concat(OutputDevices
                         .Select(x => x.Device)
                         .Cast<IDisposable>()))
        {
            device.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    [RelayCommand]
    private async Task RefreshAgentsAsync(CancellationToken cancellationToken = default)
    {
        var previouslySelectedAgent = SelectedAgent?.Agent.Name;

        Agents.Clear();

        foreach (var agent in await AgentService.GetAgentsAsync(cancellationToken: cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (agent is IDesktopAgent desktopAgent)
            {
                desktopAgent.Initialize(
                    // TODO: get Dispatcher a different way (respect MVVM)
                    Application.Current.Dispatcher,
                    SelectedInputDevice?.Device,
                    SelectedOutputDevice?.Device);
            }

            var model = agent.ToModel();
            Agents.Add(model);

            if (agent.Name == previouslySelectedAgent)
            {
                model.IsSelected = true;
                await model.Agent.OnActivatedAsync(cancellationToken);
            }
        }

        if (Agents is [{ IsSelected: false }])
        {
            await SelectAgentAsync(Agents[0], cancellationToken);
        }
    }

    [RelayCommand]
    private void RefreshDevices()
    {
        RefreshOutputDevices();
        RefreshInputDevices();
    }

    private void RefreshOutputDevices()
    {
        var previouslySelectedDeviceId = SelectedOutputDevice?.Device.Id;
        var defaultDeviceId = AudioService.GetDefaultOutputDevice().Id;

        OutputDevices.Clear();

        foreach (var item in AudioService.GetOutputDevices())
        {
            var model = item.ToModel();

            OutputDevices.Add(model);

            model.IsSelected = item.Id == previouslySelectedDeviceId || item.Id == defaultDeviceId;
        }

        if (OutputDevices is [{ IsSelected: false }])
        {
            OutputDevices[0].IsSelected = true;
        }
    }

    private void RefreshInputDevices()
    {
        var previouslySelectedDeviceId = SelectedInputDevice?.Device.Id;
        var defaultDeviceId = AudioService.GetDefaultInputDevice().Id;

        InputDevices.Clear();

        foreach (var item in AudioService.GetInputDevices())
        {
            var model = item.ToModel();

            InputDevices.Add(model);

            if (item.Id == previouslySelectedDeviceId || item.Id == defaultDeviceId)
            {
                SelectInputDevice(model);
            }
        }

        if (InputDevices is [{ IsSelected: false }])
        {
            SelectInputDevice(InputDevices[0]);
        }
    }

    [RelayCommand]
    private async Task SelectAgentAsync(AgentModel model, CancellationToken cancellationToken = default)
    {
        foreach (var item in Agents.Where(x => x != model))
        {
            if (item.IsSelected)
            {
                item.IsSelected = false;

                await item.Agent.OnDeactivatedAsync(cancellationToken);
            }
        }

        model.IsSelected = true;

        await model.Agent.OnActivatedAsync(cancellationToken);
    }

    [RelayCommand]
    private void SelectInputDevice(AudioInputDeviceModel model)
    {
        foreach (var device in InputDevices.Where(x => x != model))
        {
            device.Device.StopAll();
            device.IsSelected = false;
        }

        model.IsSelected = true;
        model.Device.StartListening();

        foreach (var agent in Agents)
        {
            if (agent.Agent is not IDesktopAgent desktopAgent)
            {
                continue;
            }

            desktopAgent.CurrentInputDevice = model.Device;
        }
    }

    [RelayCommand]
    private void SelectOutputDevice(AudioOutputDeviceModel model)
    {
        foreach (var device in OutputDevices.Where(x => x != model))
        {
            device.Device.Stop();
            device.IsSelected = false;
        }

        model.IsSelected = true;

        foreach (var agent in Agents)
        {
            if (agent.Agent is not IDesktopAgent desktopAgent)
            {
                continue;
            }

            desktopAgent.CurrentOutputDevice = model.Device;
        }
    }

    [RelayCommand]
    private void StartRecording()
    {
        var device = SelectedInputDevice?.Device;

        device?.StartRecording();
    }

    [RelayCommand]
    private async Task StopRecordingAsync()
    {
        var device = SelectedInputDevice?.Device;

        var data = device?.StopRecording();

        if (data == null || data.Length == 0)
        {
            return;
        }

        var message = await RemoteTextClientService.TranscribeAsync(data);

        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning("Failed to transcribe any audio");
            return;
        }

        _logger.LogInformation("Sending audio (transcribed): \"{message}\"", message);

        switch (SelectedMode)
        {
            case Mode.Chat:
                await DoChatAsync(message);
                break;
            case Mode.Command:
                await DoCommandAsync(message);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task DoChatAsync(string message)
    {
        if (SelectedAgent?.Agent is not IDesktopAgent agent)
        {
            return;
        }

        _ = await agent.ChatAsync(message);
    }

    // ReSharper disable once UnusedMember.Local
    private async Task DoCommandAsync(string message)
    {
        if (SelectedAgent?.Agent is not { } agent)
        {
            return;
        }

        _ = await agent.InvokeAsync(message);
    }
}