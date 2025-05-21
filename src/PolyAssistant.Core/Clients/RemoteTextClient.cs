using System.Net.Mime;
using System.Text.Json;
using PolyAssistant.Core.Clients.Interfaces;
using PolyAssistant.Core.Models.Text;

namespace PolyAssistant.Core.Clients;

public sealed class RemoteTextClient : IRemoteTextClient
{
    private readonly HttpClient _httpClient;

    public RemoteTextClient(string url, HttpClient httpClient)
    {
        _httpClient = httpClient;

        Url = url;

        if (!Url.EndsWith('/'))
        {
            Url += '/';
        }
    }

    public string Url { get; }

    public async Task OcrAsync(CancellationToken cancellationToken = default)
    {
        // api/text/ocr

        throw new NotImplementedException();
    }

    public async Task<string?> TranscribeAsync(string wavFilePath, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(wavFilePath, FileMode.OpenOrCreate, FileAccess.Read);
        return await TranscribeAsync(stream, cancellationToken);
    }

    public async Task<string?> TranscribeAsync(byte[] wavData, CancellationToken cancellationToken = default)
    {
        await using Stream stream = new MemoryStream(wavData);
        return await TranscribeAsync(stream, cancellationToken);
    }

    public async Task<string?> TranscribeAsync(Stream wavStream, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/text/transcribe");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(wavStream), "wavFile", "wavFile");

        request.Content = content;

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<string?> TranslateAsync(TextTranslationQueryModel query, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/text/translate");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var jsonContent = JsonSerializer.Serialize(query);
        request.Content = new StringContent(jsonContent, null, MediaTypeNames.Application.Json);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}