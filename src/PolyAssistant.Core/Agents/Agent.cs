using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PolyAssistant.Core.Agents.Interfaces;
using PolyAssistant.Core.Clients.Interfaces;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Agents;

public abstract class Agent<T> : IAgent where T : class
{
    private readonly string _defaultSystemMessage;
    private ChatHistory _chatHistory = [];
    private string _lastSystemMessage;

    protected Agent(ILogger<T> logger, IServiceProvider serviceProvider)
    {
        Logger = logger;

        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        var ollamaConfiguration = serviceProvider.GetRequiredService<IOptions<OllamaConfiguration>>();
        var url = ollamaConfiguration.Value.Url;
        var model = ollamaConfiguration.Value.DefaultModel;

        _defaultSystemMessage = ollamaConfiguration.Value.DefaultSystemMessage;
        _lastSystemMessage = _defaultSystemMessage;

        _chatHistory.AddSystemMessage(_defaultSystemMessage);

        var httpClient = httpClientFactory.CreateClient();

        RemoteChatClient = serviceProvider.GetRequiredService<IRemoteChatClientService>();
        RemoteFilesClient = serviceProvider.GetRequiredService<IRemoteFilesClientService>();
        RemoteImageClient = serviceProvider.GetRequiredService<IRemoteImageClientService>();
        RemoteTextClient = serviceProvider.GetRequiredService<IRemoteTextClientService>();
        RemoteTimeClient = serviceProvider.GetRequiredService<IRemoteTimeClientService>();
        RemoteVoiceClient = serviceProvider.GetRequiredService<IRemoteVoiceClientService>();

        Kernel =
            Kernel
                .CreateBuilder()
                .AddOpenAIChatCompletion(
                    model,
                    apiKey: null,
                    endpoint: new Uri($"{url}/v1"),
                    httpClient: httpClient)
                .Build();

        Kernel.FunctionInvocationFilters.Add(this);
    }

    protected ILogger<T> Logger { get; }

    protected Kernel Kernel { get; }

    protected IRemoteChatClient RemoteChatClient { get; }

    protected IRemoteFilesClient RemoteFilesClient { get; }

    protected IRemoteImageClient RemoteImageClient { get; }

    protected IRemoteTextClient RemoteTextClient { get; }

    protected IRemoteTimeClient RemoteTimeClient { get; }

    protected IRemoteVoiceClient RemoteVoiceClient { get; }

    protected virtual PromptExecutionSettings DefaultChatPromptExecutionSettings => new OpenAIPromptExecutionSettings();

    protected virtual PromptExecutionSettings DefaultInvocationPromptExecutionSettings => new OpenAIPromptExecutionSettings
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.None(),
        Temperature = 0,
        TopP = 0
    };

    public abstract string Name { get; }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var pluginName = Name.ToNormalizedPluginName();

        Kernel.Plugins.AddFromObject(this, pluginName);

        // notify
        return OnInitializedAsync(cancellationToken);
    }

    public virtual async Task<ChatMessageContent?> ChatAsync(string message, string? systemMessage = null, PromptExecutionSettings? promptExecutionSettings = null, CancellationToken cancellationToken = default)
    {
        promptExecutionSettings ??= DefaultChatPromptExecutionSettings;

        var service = Kernel.GetRequiredService<IChatCompletionService>();

        if (!string.IsNullOrWhiteSpace(systemMessage))
        {
            _chatHistory.AddSystemMessage(systemMessage);
            _lastSystemMessage = systemMessage;
        }
        else if (!_lastSystemMessage.Equals(_defaultSystemMessage, StringComparison.OrdinalIgnoreCase))
        {
            _chatHistory.AddSystemMessage(_defaultSystemMessage);
            _lastSystemMessage = _defaultSystemMessage;
        }

        _chatHistory.AddUserMessage(message);

        var result = await service.GetChatMessageContentAsync(_chatHistory, promptExecutionSettings, Kernel, cancellationToken);

        _chatHistory.Add(result);

        return result;
    }

    public Task<object?> InvokeAsync(string prompt, string? systemMessage = null, PromptExecutionSettings? promptExecutionSettings = null, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<object?>(prompt, systemMessage, promptExecutionSettings, cancellationToken);
    }

    public async Task<TValue?> InvokeAsync<TValue>(string prompt, string? systemMessage = null, PromptExecutionSettings? promptExecutionSettings = null, CancellationToken cancellationToken = default)
    {
        var rawPrompt = prompt;

        prompt = $"When parsing the following function invocation prompt, be strict in your interpretation. " +
                 $"{Environment.NewLine}{Environment.NewLine}" +
                 $"{rawPrompt}";

        promptExecutionSettings ??= DefaultInvocationPromptExecutionSettings;

        var chatCompletionService = Kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(prompt);

        if (!string.IsNullOrWhiteSpace(systemMessage))
        {
            chatHistory.AddSystemMessage(systemMessage);
        }

        var response = await chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            promptExecutionSettings,
            Kernel,
            cancellationToken);

        if (response is not OpenAIChatMessageContent openAiResponse)
        {
            return default;
        }

        var functions = openAiResponse.GetOpenAIFunctionToolCalls();

        // ReSharper disable once StringLiteralTypo
        if (functions.Count > 0 && functions[0].FunctionName.Equals("tostring", StringComparison.OrdinalIgnoreCase))
        {
            //
            // TODO: maybe find a different way to do this... (doesn't work most of the time)
            //

            // TODO: make configurable
            await OnUnrecognizedInvocationAsync(prompt, cancellationToken);

            return default;
        }

        var lastResult = default(TValue);

        foreach (var item in functions)
        {
            // NOTE: notice use of rawPrompt
            if (!await OnVerifyInvocationAsync(rawPrompt, item, chatCompletionService, cancellationToken))
            {
                // TODO: make configurable
                await OnUnrecognizedInvocationAsync(rawPrompt, cancellationToken);

                continue;
            }

            lastResult = await Kernel.InvokeAsync<TValue>(
                item.PluginName,
                item.FunctionName,
                item.Arguments == null
                    ? null
                    : new KernelArguments(item.Arguments),
                cancellationToken);
        }

        return lastResult;
    }

    public virtual Task SaveStateAsync(string path, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(_chatHistory);
        return File.WriteAllTextAsync(path, json, cancellationToken);
    }

    public virtual async Task LoadStateAsync(string path, CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(path, cancellationToken);
        _chatHistory = JsonSerializer.Deserialize<ChatHistory>(json) ?? throw new JsonException("Failed to deserialize ChatHistory");
    }

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        if (!await OnPreviewFunctionInvocationAsync(context))
        {
            return;
        }

        Logger.LogInformation("Invoked function: {functionName}()", context.Function.Name);

        await next(context);
    }

    public virtual Task OnLoadedAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task OnInitializedAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnActivatedAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnDeactivatedAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    // TODO: configure idle time in app
    public virtual Task OnIdleAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task OnNotIdleAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    // TODO: UI automation via FlaUI (https://github.com/FlaUI/FlaUI)
    //
    //

    protected virtual Task OnUnrecognizedInvocationAsync(string prompt, CancellationToken cancellationToken = default)
    {
        throw new AgentInvocationException($"Unrecognized invocation from prompt: {prompt}");
    }

    protected virtual Task<bool> OnPreviewFunctionInvocationAsync(FunctionInvocationContext context)
    {
        return Task.FromResult(true);
    }

    protected virtual Task<bool> OnVerifyInvocationAsync(string prompt, OpenAIFunctionToolCall toolCall, IChatCompletionService chatCompletionService, CancellationToken cancellationToken = default)
    {
        // return CheckInvocationCommandAsync(prompt, toolCall, chatCompletionService, cancellationToken);

        var score = prompt.CalculateSimilarity(toolCall.FunctionName);
        var result = score >= 0.45;

        return Task.FromResult(result);
    }

    [Obsolete("Unsure if this works well or not yet")]
    private async Task<bool> CheckInvocationCommandAsync(string prompt, OpenAIFunctionToolCall toolCall, IChatCompletionService chatCompletionService, CancellationToken cancellationToken = default)
    {
        var rawPrompt = prompt;

        var functionList = Kernel.Plugins.First().GetFunctionsMetadata().Select(x => x.Name).ToArray();
        var functionListStr = string.Join(Environment.NewLine, functionList);

        prompt =
            $"""
             Consider the following list of available functions:
             {functionListStr}
             Which function does the following prompt represent?
             "{rawPrompt}"
             Only respond with the function name. Nothing else.
             Be strict in your matching.
             If you cannot find a suitable match, respond with NULL.
             """;

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(prompt);

        var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

        var val = response.Content;

        return val != null;
    }

    // so derived classes aren't required to have any KernelFunction methods
    [KernelFunction]
    public override string? ToString()
    {
        return null;
    }
}