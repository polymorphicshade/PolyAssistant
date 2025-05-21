using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using PolyAssistant.Core.Agents;
using PolyAssistant.Core.Agents.Interfaces;
using PolyAssistant.Core.Models.Chat;
using PolyAssistant.Core.Models.Voice;
using PolyAssistant.Desktop.Agents.Interfaces;
using PolyAssistant.Desktop.Components.Interfaces;
using PolyAssistant.Desktop.Services.Interfaces;

namespace PolyAssistant.Desktop.Agents;

public abstract class DesktopAgent<T>(ILogger<T> logger, IServiceProvider serviceProvider) : Agent<T>(logger, serviceProvider), IDesktopAgent where T : class
{
    private Dispatcher? _dispatcher;

    protected virtual IAgentVoice? DefaultVoice => null;

    protected virtual IAgentVoice? DefaultVoiceForUnrecognizedInvocation => null;

    public Dispatcher Dispatcher => _dispatcher ?? throw new InvalidOperationException("Agent not initialized yet");

    public IAudioInputDevice? CurrentInputDevice { get; set; }

    public IAudioOutputDevice? CurrentOutputDevice { get; set; }

    public IAutomationService Automation { get; } = serviceProvider.GetRequiredService<IAutomationService>();

    void IDesktopAgent.Initialize(Dispatcher dispatcher, IAudioInputDevice? currentInputDevice, IAudioOutputDevice? currentOutputDevice)
    {
        _dispatcher = dispatcher;
        CurrentInputDevice = currentInputDevice;
        CurrentOutputDevice = currentOutputDevice;
    }

    public virtual async Task<ChatMessageContent?> ChatAsync(string message, string? systemMessage = null, PromptExecutionSettings? promptExecutionSettings = null, IAgentVoice? voice = null, CancellationToken cancellationToken = default)
    {
        var response = await base.ChatAsync(message, systemMessage, promptExecutionSettings, cancellationToken);

        var responseMessage = response?.Content;

        if (string.IsNullOrWhiteSpace(responseMessage))
        {
            Logger.LogWarning("Received an empty response");
            return null;
        }

        Logger.LogInformation("Response: \"{message}\"", responseMessage);

        await SpeakAsync(responseMessage, DefaultVoice, cancellationToken);

        return response;
    }

    public virtual async Task SpeakAsync(string text, IAgentVoice? voice = null, CancellationToken cancellationToken = default)
    {
        switch (voice ?? DefaultVoice)
        {
            case IZonosAgentVoice zonosAgentVoice:
                await SpeakWithZonosAsync(text, zonosAgentVoice, cancellationToken);
                break;
            case IChatterboxAgentVoice chatterboxAgentVoice:
                await SpeakWithChatterboxAsync(text, chatterboxAgentVoice, cancellationToken);
                break;
        }
    }

    public virtual async Task SpeakAsync(WordCategory category, IAgentVoice? voice = null, CancellationToken cancellationToken = default)
    {
        string[] phrases = category switch
        {
            WordCategory.Ok => ["okay", "right away", "yep", "sure", "you bet", "of course", "Mhm...", "alright"],
            WordCategory.Rejection => ["sorry", "no", "nope", "I can't do that"],
            WordCategory.Oops => ["oops!", "sorry!", "my bad", "sorry about that..."],
            WordCategory.UnrecognizedInvocation => ["Sorry... what?", "What was that?", "What did you say?", "Sorry what?", "Um... I didn't quite catch that.", "Could you repeat that?"],
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };

        await SpeakRandomAsync(phrases, voice, cancellationToken);
    }

    protected override async Task OnUnrecognizedInvocationAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var voice = DefaultVoiceForUnrecognizedInvocation ?? DefaultVoice;

        await SpeakAsync("Sorry... what did you say?", voice, cancellationToken);
    }

    // helpers

    private async Task SpeakWithZonosAsync(string text, IZonosAgentVoice voice, CancellationToken cancellationToken = default)
    {
        if (CurrentOutputDevice == null)
        {
            Logger.LogWarning("Tried to speak without an output device");
            return;
        }

        var voiceResponse = await RemoteVoiceClient.GenerateAsync(new VoiceGenerationQueryModel(text)
        {
            VoiceFilePath = voice.VoiceFilePath,
            Language = voice.Language,
            Happiness = voice.Happiness,
            Sadness = voice.Sadness,
            Disgust = voice.Disgust,
            Fear = voice.Fear,
            Surprise = voice.Surprise,
            Anger = voice.Anger,
            Other = voice.Other,
            Neutral = voice.Neutral,
            PitchStd = voice.PitchStd,
            SpeakingRate = voice.SpeakingRate
        }, cancellationToken);

        await Dispatcher.InvokeAsync(() => _ = CurrentOutputDevice.PlayAsync(voiceResponse, cancellationToken));
    }

    private async Task SpeakWithChatterboxAsync(string text, IChatterboxAgentVoice voice, CancellationToken cancellationToken = default)
    {
        if (CurrentOutputDevice == null)
        {
            Logger.LogWarning("Tried to speak without an output device");
            return;
        }

        var voiceResponse = await RemoteVoiceClient.GenerateExAsync(new VoiceGenerationExQueryModel(text)
        {
            VoiceFilePath = voice.VoiceFilePath,
            Seed = 0,
            Exaggeration = voice.Exaggeration,
            Pace = voice.Pace,
            Temperature = voice.Temperature
        }, cancellationToken);

        await Dispatcher.InvokeAsync(() => _ = CurrentOutputDevice.PlayAsync(voiceResponse, cancellationToken));
    }

    private async Task SpeakRandomAsync(string[] phrases, IAgentVoice? voice = null, CancellationToken cancellationToken = default)
    {
        if (CurrentOutputDevice == null)
        {
            Logger.LogWarning("Tried to speak without an output device");
            return;
        }

        var response = await RemoteChatClient.SendAsync(new ChatQueryModel
        {
            IsAtomic = true,
            Message =
                $"Pick a phrase from the following CSV: {string.Join(',', phrases)}" +
                $"{Environment.NewLine}Only provide the phrase. Nothing else." +
                $"{Environment.NewLine}Do not embellish your response."
        }, cancellationToken);

        var responseMessage = response.Message?.Trim();

        if (string.IsNullOrWhiteSpace(responseMessage))
        {
            Logger.LogWarning("Received an empty response");
            return;
        }

        Logger.LogInformation("Received: {str}", responseMessage);

        await SpeakAsync(responseMessage, voice, cancellationToken);
    }
}