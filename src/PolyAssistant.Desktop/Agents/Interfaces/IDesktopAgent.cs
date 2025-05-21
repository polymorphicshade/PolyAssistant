using Microsoft.SemanticKernel;
using PolyAssistant.Core.Agents.Interfaces;
using PolyAssistant.Desktop.Components.Interfaces;
using PolyAssistant.Desktop.Services.Interfaces;
using System.Windows.Threading;

namespace PolyAssistant.Desktop.Agents.Interfaces;

public interface IDesktopAgent : IAgent
{
    Dispatcher Dispatcher { get; }

    IAudioInputDevice? CurrentInputDevice { get; set; }

    IAudioOutputDevice? CurrentOutputDevice { get; set; }

    IAutomationService Automation { get; }

    void Initialize(Dispatcher dispatcher, IAudioInputDevice? currentInputDevice, IAudioOutputDevice? currentOutputDevice);

    Task<ChatMessageContent?> ChatAsync(string message, string? systemMessage = null, PromptExecutionSettings? promptExecutionSettings = null, IAgentVoice? voice = null, CancellationToken cancellationToken = default);

    Task SpeakAsync(string text, IAgentVoice? voice = null, CancellationToken cancellationToken = default);

    Task SpeakAsync(WordCategory category, IAgentVoice? voice = null, CancellationToken cancellationToken = default);
}