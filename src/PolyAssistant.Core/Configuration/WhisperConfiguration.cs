namespace PolyAssistant.Core.Configuration;

public sealed class WhisperConfiguration
{
    public const string Key = "Whisper";

    public string Url { get; set; } = string.Empty;
}