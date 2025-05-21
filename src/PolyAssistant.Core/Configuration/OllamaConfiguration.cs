namespace PolyAssistant.Core.Configuration;

public class OllamaConfiguration
{
    public const string Key = "Ollama";

    public string Url { get; set; } = string.Empty;

    public string DefaultModel { get; set; } = "llama3.2:latest";

    public string DefaultVisionModel { get; set; } = "qwen2.5vl:32b";

    public string DefaultSystemMessage { get; set; } = "You are a friendly assistant. Respond in sentence-format (they should be suitable for TTS).";
}