namespace PolyAssistant.Core.Configuration;

public class FileManagementConfiguration
{
    public const string Key = "Files";

    // TODO: change to "drop"
    public string RootDirectoryPath { get; set; } = "uploads";
}