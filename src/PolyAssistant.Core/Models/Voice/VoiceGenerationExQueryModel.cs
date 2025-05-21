using System.ComponentModel;

namespace PolyAssistant.Core.Models.Voice;

public sealed class VoiceGenerationExQueryModel(string text)
{
    [DefaultValue("Hello there! How are you?")]
    public string Text { get; } = text;

    [DefaultValue("/path/to/voice_sample.mp3")]
    public string? VoiceFilePath { get; set; }

    [DefaultValue(null)]
    public int? Seed { get; set; }

    [DefaultValue(0.5)]
    public double Exaggeration { get; set; } = 0.5;

    [DefaultValue(0.5)]
    public double Pace { get; set; } = 0.5;

    [DefaultValue(0.8)]
    public double Temperature { get; set; } = 0.8;
}