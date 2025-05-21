using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyAssistant.Core.Clients;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Models.Text;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Services;

public class RemoteTextClientService(ILogger<RemoteTextClientService> logger, IHttpClientFactory httpClientFactory, IOptions<PolyAssistantConfiguration> configuration) : IRemoteTextClientService
{
    public string Url => configuration.Value.Url;

    public Task OcrAsync(CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.OcrAsync(cancellationToken);
    }

    public Task<string?> TranscribeAsync(string wavFilePath, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.TranscribeAsync(wavFilePath, cancellationToken);
    }

    public Task<string?> TranscribeAsync(byte[] wavData, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.TranscribeAsync(wavData, cancellationToken);
    }

    public Task<string?> TranscribeAsync(Stream wavStream, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.TranscribeAsync(wavStream, cancellationToken);
    }

    public Task<string?> TranslateAsync(TextTranslationQueryModel query, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.TranslateAsync(query, cancellationToken);
    }

    // helpers

    private RemoteTextClient GetClient()
    {
        return new RemoteTextClient(Url, httpClientFactory.CreateClient());
    }
}