namespace PolyAssistant.Core.Configuration;

public sealed class AgentServiceConfiguration
{
    public const string Key = "Agents";

    public string DefaultPluginDirectoryPath { get; set; } = "plugins";
}