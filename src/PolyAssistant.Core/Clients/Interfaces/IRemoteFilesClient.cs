using PolyAssistant.Core.Models.Files;

namespace PolyAssistant.Core.Clients.Interfaces;

public interface IRemoteFilesClient
{
    string Url { get; }

    Task DeleteFileAsync(FileDeleteQueryModel query, CancellationToken cancellationToken = default);

    Task<string[]> FindFilesAsync(FileQueryModel query, CancellationToken cancellationToken = default);

    Task UploadFileAsync(string localFilePath, string remoteFilePath, CancellationToken cancellationToken = default);

    Task UploadFileAsync(Stream stream, string remoteFilePath, CancellationToken cancellationToken = default);
}