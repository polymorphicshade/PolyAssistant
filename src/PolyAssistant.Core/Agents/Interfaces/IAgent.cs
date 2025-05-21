using Microsoft.SemanticKernel;

namespace PolyAssistant.Core.Agents.Interfaces;

public interface IAgent : IFunctionInvocationFilter
{
    string Name { get; }

    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<ChatMessageContent?> ChatAsync(string message, string? systemMessage = null, PromptExecutionSettings? promptExecutionSettings = null, CancellationToken cancellationToken = default);

    Task<object?> InvokeAsync(string prompt, string? systemMessage = null, PromptExecutionSettings? promptExecutionSettings = null, CancellationToken cancellationToken = default);

    Task<TValue?> InvokeAsync<TValue>(string prompt, string? systemMessage = null, PromptExecutionSettings? promptExecutionSettings = null, CancellationToken cancellationToken = default);

    Task SaveStateAsync(string path, CancellationToken cancellationToken = default);

    Task LoadStateAsync(string path, CancellationToken cancellationToken = default);

    Task OnLoadedAsync(CancellationToken cancellationToken = default);

    Task OnInitializedAsync(CancellationToken cancellationToken = default);

    Task OnActivatedAsync(CancellationToken cancellationToken = default);

    Task OnDeactivatedAsync(CancellationToken cancellationToken = default);

    Task OnIdleAsync(CancellationToken cancellationToken = default);

    Task OnNotIdleAsync(CancellationToken cancellationToken = default);
}