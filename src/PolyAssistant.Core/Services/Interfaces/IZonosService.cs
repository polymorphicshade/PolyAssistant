// ReSharper disable IdentifierTypo

namespace PolyAssistant.Core.Services.Interfaces;

public interface IZonosService
{
    Task<byte[]> ProcessAsync(
        string text, string language = "en-us", string? voiceFilePath = null,
        string modelChoice = "Zyphra/Zonos-v0.1-hybrid",
        string? prefixAudioPath = null,
        float happiness = 1.0f, float sadness = 0.05f, float disgust = 0.05f, float fear = 0.05f, float surprise = 0.05f, float anger = 0.05f, float other = 0.1f, float neutral = 0.2f,
        float vqScore = 0.78f, float maxFrequency = 24000f, float pitchStd = 45.0f, float speakingRate = 15.0f, float dnsmosOverall = 4.0f, bool denoiseSpeaker = false,
        float cfgScale = 2.0f, float samplingTopP = 0.0f, int samplingTopK = 0, float samplingMinP = 0.0f, float samplingLinear = 0.5f, float samplingConfidence = 0.4f, float samplingQuadratic = 0.0f,
        int seed = 420, bool randomizeSeed = false, string[]? unconditionalKeys = null,
        CancellationToken cancellationToken = default);
}