﻿using PolyAssistant.Core.Models.Files;

namespace PolyAssistant.Core.Services.Interfaces;

public interface IFileSystemService
{
    DirectoryInfo RootDirectory { get; }

    IEnumerable<string> FindFiles(FileQueryModel query);

    Task SaveFileAsync(byte[] bytes, string path);

    bool DeleteFile(string path);

    Task<byte[]?> ReadFileBytesAsync(string path, CancellationToken cancellationToken = default);
}