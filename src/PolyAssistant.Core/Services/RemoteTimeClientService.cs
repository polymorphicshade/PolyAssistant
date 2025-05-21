using Microsoft.Extensions.Options;
using PolyAssistant.Core.Clients;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Models.Time;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Services;

public sealed class RemoteTimeClientService(IHttpClientFactory httpClientFactory, IOptions<PolyAssistantConfiguration> configuration) : IRemoteTimeClientService
{
    public string Url => configuration.Value.Url;

    public Task<string> GetCurrentTimeAsync(string? timeZone = null, string? stringFormat = null, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.GetCurrentTimeAsync(timeZone, stringFormat, cancellationToken);
    }

    public Task<TimeZoneInfoModel[]> GetTimeZonesAsync(CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.GetTimeZonesAsync(cancellationToken);
    }

    // helpers

    private RemoteTimeClient GetClient()
    {
        return new RemoteTimeClient(Url, httpClientFactory.CreateClient());
    }
}