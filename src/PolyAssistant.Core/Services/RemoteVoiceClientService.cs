using Microsoft.Extensions.Options;
using PolyAssistant.Core.Clients;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Models.Voice;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Services;

public sealed class RemoteVoiceClientService(IHttpClientFactory httpClientFactory, IOptions<PolyAssistantConfiguration> configuration) : IRemoteVoiceClientService
{
    public string Url => configuration.Value.Url;

    public Task ChangeAsync(CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.ChangeAsync(cancellationToken);
    }

    public Task<byte[]> GenerateAsync(VoiceGenerationQueryModel query, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.GenerateAsync(query, cancellationToken);
    }

    public Task<byte[]> GenerateExAsync(VoiceGenerationExQueryModel query, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.GenerateExAsync(query, cancellationToken);
    }

    // helpers

    private RemoteVoiceClient GetClient()
    {
        return new RemoteVoiceClient(Url, httpClientFactory.CreateClient());
    }
}