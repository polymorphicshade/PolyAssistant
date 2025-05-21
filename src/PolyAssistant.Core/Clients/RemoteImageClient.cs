using System.Net.Mime;
using System.Text.Json;
using PolyAssistant.Core.Clients.Interfaces;
using PolyAssistant.Core.Models.Chat;
using PolyAssistant.Core.Models.Image;

namespace PolyAssistant.Core.Clients;

public sealed class RemoteImageClient : IRemoteImageClient
{
    private readonly HttpClient _httpClient;

    public RemoteImageClient(string url, HttpClient httpClient)
    {
        _httpClient = httpClient;

        Url = url;

        if (!Url.EndsWith('/'))
        {
            Url += '/';
        }
    }

    public string Url { get; }

    public async Task<ChatResponseModel> QueryAsync(ImageQueryModel query, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/image/query");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var jsonContent = JsonSerializer.Serialize(query);
        request.Content = new StringContent(jsonContent, null, MediaTypeNames.Application.Json);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<ChatResponseModel>(json, JsonSerializerOptions.Web) ?? throw new JsonException("Invalid JSON");
    }

    public async Task<string[]> TagAsync(ImageTagQueryModel query, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/image/tag");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var jsonContent = JsonSerializer.Serialize(query);
        request.Content = new StringContent(jsonContent, null, MediaTypeNames.Application.Json);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<string[]>(json, JsonSerializerOptions.Web) ?? throw new JsonException("Invalid JSON");
    }
}