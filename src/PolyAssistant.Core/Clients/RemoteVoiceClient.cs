using System.Net.Mime;
using System.Text.Json;
using PolyAssistant.Core.Clients.Interfaces;
using PolyAssistant.Core.Models.Voice;

namespace PolyAssistant.Core.Clients;

public sealed class RemoteVoiceClient : IRemoteVoiceClient
{
    private readonly HttpClient _httpClient;

    public RemoteVoiceClient(string url, HttpClient httpClient)
    {
        _httpClient = httpClient;

        Url = url;

        if (!Url.EndsWith('/'))
        {
            Url += '/';
        }
    }

    public string Url { get; }

    public Task ChangeAsync(CancellationToken cancellationToken = default)
    {
        // api/voice/change

        throw new NotImplementedException();
    }

    public async Task<byte[]> GenerateAsync(VoiceGenerationQueryModel query, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/voice/generate");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var jsonContent = JsonSerializer.Serialize(query);
        request.Content = new StringContent(jsonContent, null, MediaTypeNames.Application.Json);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<byte[]> GenerateExAsync(VoiceGenerationExQueryModel query, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/voice/generate_ex");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var jsonContent = JsonSerializer.Serialize(query);
        request.Content = new StringContent(jsonContent, null, MediaTypeNames.Application.Json);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}