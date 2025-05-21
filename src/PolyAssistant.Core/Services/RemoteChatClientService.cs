using Microsoft.Extensions.Options;
using PolyAssistant.Core.Clients;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Models.Chat;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Services;

public class RemoteChatClientService(IHttpClientFactory httpClientFactory, IOptions<PolyAssistantConfiguration> configuration) : IRemoteChatClientService
{
    public string Url => configuration.Value.Url;

    public Task ClearAsync(IEnumerable<Guid>? conversationIds, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.ClearAsync(conversationIds, cancellationToken);
    }

    public Task<ChatConversationModel[]> FindAsync(ChatFindQueryModel query, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.FindAsync(query, cancellationToken);
    }

    public Task<ChatResponseModel> SendAsync(ChatQueryModel query, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.SendAsync(query, cancellationToken);
    }

    public Task<ChatConversationModel> ResetAsync(ChatResetQueryModel query, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.ResetAsync(query, cancellationToken);
    }

    // helpers

    private RemoteChatClient GetClient()
    {
        return new RemoteChatClient(Url, httpClientFactory.CreateClient());
    }
}