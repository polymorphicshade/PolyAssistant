using Microsoft.Extensions.Options;
using PolyAssistant.Core.Clients;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Models.Chat;
using PolyAssistant.Core.Models.Image;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Services;

public sealed class RemoteImageClientService(IHttpClientFactory httpClientFactory, IOptions<PolyAssistantConfiguration> configuration) : IRemoteImageClientService
{
    public string Url => configuration.Value.Url;

    public Task<ChatResponseModel> QueryAsync(ImageQueryModel query, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.QueryAsync(query, cancellationToken);
    }

    public Task<string[]> TagAsync(ImageTagQueryModel query, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.TagAsync(query, cancellationToken);
    }

    // helpers

    private RemoteImageClient GetClient()
    {
        return new RemoteImageClient(Url, httpClientFactory.CreateClient());
    }
}