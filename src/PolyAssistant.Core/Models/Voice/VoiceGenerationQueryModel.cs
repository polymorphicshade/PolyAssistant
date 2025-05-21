using System.ComponentModel;

namespace PolyAssistant.Core.Models.Voice;

public sealed class VoiceGenerationQueryModel(string text)
{
    [DefaultValue("Hello there! How are you?")]
    public string Text { get; } = text;

    [DefaultValue("/path/to/voice_sample.mp3")]
    public string? VoiceFilePath { get; set; }

    [DefaultValue("Zyphra/Zonos-v0.1-hybrid")]
    public string Model { get; set; } = "Zyphra/Zonos-v0.1-hybrid";

    [DefaultValue("en-us")]
    public string Language { get; set; } = "en-us";

    [DefaultValue(null)]
    public string? PrefixAudioPath { get; set; }

    [DefaultValue(1.0f)]
    public float Happiness { get; set; } = 1.0f;

    [DefaultValue(0.5f)]
    public float Sadness { get; set; } = 0.05f;

    [DefaultValue(0.5f)]
    public float Disgust { get; set; } = 0.05f;

    [DefaultValue(0.5f)]
    public float Fear { get; set; } = 0.05f;

    [DefaultValue(0.5f)]
    public float Surprise { get; set; } = 0.05f;

    [DefaultValue(0.5f)]
    public float Anger { get; set; } = 0.05f;

    [DefaultValue(0.1f)]
    public float Other { get; set; } = 0.1f;

    [DefaultValue(0.2f)]
    public float Neutral { get; set; } = 0.2f;

    [DefaultValue(0.78f)]
    public float VqScore { get; set; } = 0.78f;

    [DefaultValue(24000f)]
    public float MaxFrequency { get; set; } = 24000f;

    [DefaultValue(45.0f)]
    public float PitchStd { get; set; } = 45.0f;

    [DefaultValue(15.0f)]
    public float SpeakingRate { get; set; } = 15.0f;

    [DefaultValue(4.0f)]
    public float DnsmosOverall { get; set; } = 4.0f;

    [DefaultValue(true)]
    public bool DeNoiseSpeaker { get; set; } = true;

    [DefaultValue(2.0f)]
    public float CfgScale { get; set; } = 2.0f;

    [DefaultValue(0.0f)]
    public float SamplingTopP { get; set; } = 0.0f;

    [DefaultValue(0)]
    public int SamplingTopK { get; set; } = 0;

    [DefaultValue(0.0f)]
    public float SamplingMinP { get; set; } = 0.0f;

    [DefaultValue(0.5f)]
    public float SamplingLinear { get; set; } = 0.5f;

    [DefaultValue(0.4f)]
    public float SamplingConfidence { get; set; } = 0.4f;

    [DefaultValue(0.0f)]
    public float SamplingQuadratic { get; set; } = 0.0f;

    [DefaultValue(null)]
    public int? Seed { get; set; }
}