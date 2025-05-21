using System.Net.Mime;
using System.Text.Json;
using PolyAssistant.Core.Models.Web;

namespace PolyAssistant.Core.Clients;

public sealed class RemoteWebClient
{
    private readonly HttpClient _httpClient;

    public RemoteWebClient(string url, HttpClient httpClient)
    {
        _httpClient = httpClient;

        Url = url;

        if (!Url.EndsWith('/'))
        {
            Url += '/';
        }
    }

    public string Url { get; set; }

    public async Task ScrapeAsync(CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/web/scrape");

        throw new NotImplementedException();
    }

    public async Task<SearchResultModel[]> SearchAsync(SearchQueryModel query, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/web/search");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var jsonContent = JsonSerializer.Serialize(query);
        request.Content = new StringContent(jsonContent, null, MediaTypeNames.Application.Json);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<SearchResultModel[]>(json, JsonSerializerOptions.Web) ?? throw new JsonException("Invalid JSON");
    }
}