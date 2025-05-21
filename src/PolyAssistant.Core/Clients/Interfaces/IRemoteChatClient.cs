using PolyAssistant.Core.Models.Chat;

namespace PolyAssistant.Core.Clients.Interfaces;

public interface IRemoteChatClient
{
    string Url { get; }

    Task ClearAsync(IEnumerable<Guid>? conversationIds, CancellationToken cancellationToken = default);

    Task<ChatConversationModel[]> FindAsync(ChatFindQueryModel query, CancellationToken cancellationToken = default);

    Task<ChatResponseModel> SendAsync(ChatQueryModel query, CancellationToken cancellationToken = default);

    Task<ChatConversationModel> ResetAsync(ChatResetQueryModel query, CancellationToken cancellationToken = default);
}