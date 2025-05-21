using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PolyAssistant.Core.Clients;

// see: https://github.com/searxng/searxng
public sealed class SearXngClient
{
    private readonly HttpClient _httpClient;

    public SearXngClient(string url, HttpClient httpClient, ILogger? logger = null)
    {
        Url = url;
        _httpClient = httpClient;

        Logger = logger ?? NullLogger<SearXngClient>.Instance;

        if (!Url.EndsWith('/'))
        {
            Url += '/';
        }
    }

    public ILogger Logger { get; }

    public string Url { get; }

    public async Task<JsonObject?> SearchAsync(string query, string? prefix = null, CancellationToken cancellationToken = default)
    {
        query = prefix == null ? query : $"{prefix} {query}";

        var uri = new Uri($"{Url}search?q={query}&format=json");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<JsonObject>(json);
    }
}