using Microsoft.SemanticKernel.Connectors.OpenAI;
using PolyAssistant.Core.Models.Chat;

namespace PolyAssistant.Core.Services.Interfaces;

public interface IOllamaService
{
    Task ClearConversationsAsync(IEnumerable<Guid>? conversationIds = null, CancellationToken cancellationToken = default);

    IEnumerable<ChatConversationModel> GetConversationsAsync(int conversationTopCount = 1, int messagesTopCount = 50, DateTime? conversationFrom = null, DateTime? conversationTo = null, DateTime? messageFrom = null, DateTime? messageTo = null, CancellationToken cancellationToken = default);

    IEnumerable<ChatConversationModel> GetConversationsAsync(Guid[] ids, CancellationToken cancellationToken = default);

    Task<ChatResponseModel> ChatAsync(string message, string? systemMessage = null, string? model = null, string? imageContext = null, OpenAIPromptExecutionSettings? settings = null, Guid? conversationId = null, bool isAtomic = false, CancellationToken cancellationToken = default);

    Task<ChatConversationModel?> ResetConversationAsync(Guid conversationId, long? messageId = null, CancellationToken cancellationToken = default);

    Task CreateModelAsync(string name, string modelfile, CancellationToken cancellationToken = default);

    Task DownloadModelAsync(string url, string destinationPath, CancellationToken cancellationToken = default);
}