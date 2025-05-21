using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Models.Files;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Services;

public sealed class FileSystemService : IFileSystemService
{
    private readonly ILogger<FileSystemService> _logger;

    public FileSystemService(ILogger<FileSystemService> logger, IOptions<FileManagementConfiguration> configuration)
    {
        _logger = logger;

        var rootDirectoryPath = configuration.Value.RootDirectoryPath;

        RootDirectory = Directory.CreateDirectory(rootDirectoryPath);

        logger.LogInformation("Root file path: {path}", rootDirectoryPath);

        var fileCount = Directory.GetFiles(Path.GetFullPath(rootDirectoryPath), "*.*", SearchOption.AllDirectories).Length;

        if (fileCount == 0)
        {
            logger.LogWarning("No file(s) found in root path");
        }
        else
        {
            logger.LogInformation("Found {count} file(s) in root path", fileCount);
        }
    }

    public DirectoryInfo RootDirectory { get; }

    public IEnumerable<string> FindFiles(FileQueryModel query)
    {
        var pattern = query.Pattern;

        if (string.IsNullOrWhiteSpace(pattern))
        {
            yield break;
        }

        foreach (var file in RootDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories))
        {
            var path = MakeRelativePath(RootDirectory.FullName, file.FullName);

            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            if (query.Pattern == null || Regex.IsMatch(path, query.Pattern))
            {
                yield return path;
            }
        }
    }

    public async Task SaveFileAsync(byte[] data, string path)
    {
        path = GetPathNormalized(path);

        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new ArgumentException("Could not get directory name"));

        await File.WriteAllBytesAsync(path, data);

        _logger.LogInformation("Assembly saved to: {path}", path);
    }

    public bool DeleteFile(string path)
    {
        path = GetPathNormalized(path);

        if (!File.Exists(path))
        {
            return false;
        }

        File.Delete(path);

        _logger.LogInformation("Deleted file: {path}", path);

        var directoryPath = Path.GetDirectoryName(path);

        if (!(directoryPath == null || directoryPath.Equals(RootDirectory.FullName, StringComparison.InvariantCultureIgnoreCase)))
        {
            if (Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories).Length == 0)
            {
                Directory.Delete(directoryPath);

                _logger.LogInformation("Deleted empty directory: {path}", directoryPath);
            }
        }

        return true;
    }

    public async Task<byte[]?> ReadFileBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        path = GetPathNormalized(path);

        _logger.LogInformation("Attempting to read file: {path}", path);

        if (!File.Exists(path))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(path, cancellationToken);
    }

    // helpers

    private static string MakeRelativePath(string basePath, string fullPath)
    {
        if (string.IsNullOrEmpty(basePath))
        {
            throw new ArgumentNullException(nameof(basePath));
        }

        if (string.IsNullOrEmpty(fullPath))
        {
            throw new ArgumentNullException(nameof(fullPath));
        }

        var baseUri = new Uri(basePath);
        var fullUri = new Uri(fullPath);

        // path can't be made relative.
        if (baseUri.Scheme != fullUri.Scheme)
        {
            return fullPath;
        }

        var path = fullPath[basePath.Length..];

        if (fullUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
        {
            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        if (path.StartsWith('/') || path.StartsWith('\\'))
        {
            path = path[1..];
        }
        else if (path.StartsWith("./") || path.StartsWith(".\\"))
        {
            path = path[2..];
        }
        else if (path.StartsWith("../") || path.StartsWith("..\\"))
        {
            path = path[3..];
        }

        return path;
    }

    private string GetPathNormalized(string path)
    {
        if (path.StartsWith('/') || path.StartsWith('\\'))
        {
            path = path[1..];
        }
        else if (path.StartsWith("./") || path.StartsWith(".\\"))
        {
            path = path[2..];
        }
        else if (path.StartsWith("../") || path.StartsWith("..\\"))
        {
            path = path[3..];
        }

        var result = Path.GetFullPath(Path.Combine(RootDirectory.FullName, path));

        return result;
    }
}