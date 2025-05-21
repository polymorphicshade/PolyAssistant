using Microsoft.Extensions.Options;
using PolyAssistant.Core.Clients;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Models.Files;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Services;

public sealed class RemoteFilesClientService(IHttpClientFactory httpClientFactory, IOptions<PolyAssistantConfiguration> configuration) : IRemoteFilesClientService
{
    public string Url => configuration.Value.Url;

    public Task DeleteFileAsync(FileDeleteQueryModel query, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.DeleteFileAsync(query, cancellationToken);
    }

    public Task<string[]> FindFilesAsync(FileQueryModel query, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.FindFilesAsync(query, cancellationToken);
    }

    public Task UploadFileAsync(string localFilePath, string remoteFilePath, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.UploadFileAsync(localFilePath, remoteFilePath, cancellationToken);
    }

    public Task UploadFileAsync(Stream stream, string remoteFilePath, CancellationToken cancellationToken = default)
    {
        var client = GetClient();

        return client.UploadFileAsync(stream, remoteFilePath, cancellationToken);
    }

    // helpers

    private RemoteFilesClient GetClient()
    {
        return new RemoteFilesClient(Url, httpClientFactory.CreateClient());
    }
}