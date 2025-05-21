using System.Net.Mime;
using System.Text.Json;
using PolyAssistant.Core.Clients.Interfaces;
using PolyAssistant.Core.Models.Chat;

namespace PolyAssistant.Core.Clients;

public sealed class RemoteChatClient : IRemoteChatClient
{
    private readonly HttpClient _httpClient;

    public RemoteChatClient(string url, HttpClient httpClient)
    {
        _httpClient = httpClient;

        Url = url;

        if (!Url.EndsWith('/'))
        {
            Url += '/';
        }
    }

    public string Url { get; }

    public async Task ClearAsync(IEnumerable<Guid>? conversationIds, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/chat/clear");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var jsonContent = JsonSerializer.Serialize(conversationIds);
        request.Content = new StringContent(jsonContent, null, MediaTypeNames.Application.Json);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ChatConversationModel[]> FindAsync(ChatFindQueryModel query, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/chat/find");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var jsonContent = JsonSerializer.Serialize(query);
        request.Content = new StringContent(jsonContent, null, MediaTypeNames.Application.Json);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<ChatConversationModel[]>(json, JsonSerializerOptions.Web) ?? throw new JsonException("Invalid JSON");
    }

    public async Task<ChatResponseModel> SendAsync(ChatQueryModel query, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/chat/send");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var jsonContent = JsonSerializer.Serialize(query);
        request.Content = new StringContent(jsonContent, null, MediaTypeNames.Application.Json);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<ChatResponseModel>(json, JsonSerializerOptions.Web) ?? throw new JsonException("Invalid JSON");
    }

    public async Task<ChatConversationModel> ResetAsync(ChatResetQueryModel query, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/chat/reset");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var jsonContent = JsonSerializer.Serialize(query);
        request.Content = new StringContent(jsonContent, null, MediaTypeNames.Application.Json);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<ChatConversationModel>(json, JsonSerializerOptions.Web) ?? throw new JsonException("Invalid JSON");
    }
}