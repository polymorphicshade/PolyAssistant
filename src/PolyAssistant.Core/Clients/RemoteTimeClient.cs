using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PolyAssistant.Core.Clients.Interfaces;
using PolyAssistant.Core.Models.Time;

namespace PolyAssistant.Core.Clients;

public sealed class RemoteTimeClient : IRemoteTimeClient
{
    private readonly HttpClient _httpClient;

    public RemoteTimeClient(string url, HttpClient httpClient)
    {
        _httpClient = httpClient;

        Url = url;

        if (!Url.EndsWith('/'))
        {
            Url += '/';
        }
    }

    public string Url { get; }

    public async Task<string> GetCurrentTimeAsync(string? timeZone = null, string? stringFormat = null, CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, string>();

        if (timeZone != null)
        {
            query.Add("timeZone", timeZone);
        }

        if (stringFormat != null)
        {
            query.Add("stringFormat", stringFormat);
        }

        var queryStr = QueryString.Create(query);

        var uri = new Uri($"{Url}api/time/current{queryStr}");

        var response = await _httpClient.GetAsync(uri, cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<TimeZoneInfoModel[]> GetTimeZonesAsync(CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/time/zones");

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<TimeZoneInfoModel[]>(json, JsonSerializerOptions.Web) ?? throw new JsonException("Invalid JSON");
    }
}