using System.Net.Mime;
using System.Text.Json;
using PolyAssistant.Core.Clients.Interfaces;
using PolyAssistant.Core.Models.Files;

namespace PolyAssistant.Core.Clients;

public class RemoteFilesClient : IRemoteFilesClient
{
    private readonly HttpClient _httpClient;

    public RemoteFilesClient(string url, HttpClient httpClient)
    {
        _httpClient = httpClient;

        Url = url;

        if (!Url.EndsWith('/'))
        {
            Url += '/';
        }
    }

    public string Url { get; }

    public async Task DeleteFileAsync(FileDeleteQueryModel query, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/files/delete");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var jsonContent = JsonSerializer.Serialize(query);
        request.Content = new StringContent(jsonContent, null, MediaTypeNames.Application.Json);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string[]> FindFilesAsync(FileQueryModel query, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/files/find");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var jsonContent = JsonSerializer.Serialize(query);
        request.Content = new StringContent(jsonContent, null, MediaTypeNames.Application.Json);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<string[]>(json, JsonSerializerOptions.Web) ?? throw new JsonException("Invalid JSON");
    }

    public async Task UploadFileAsync(string localFilePath, string remoteFilePath, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(localFilePath, FileMode.OpenOrCreate, FileAccess.Read);

        await UploadFileAsync(stream, remoteFilePath, cancellationToken);
    }

    public async Task UploadFileAsync(Stream stream, string remoteFilePath, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{Url}api/files/upload");

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(stream), "file", "file");
        content.Add(new StringContent(remoteFilePath), "FilePath");

        request.Content = content;

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}