namespace PolyAssistant.Core.Configuration;

public class FileManagementConfiguration
{
    public const string Key = "Files";

    public string RootDirectoryPath { get; set; } = "uploads";
}