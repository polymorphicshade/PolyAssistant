using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Services;

public sealed class ZonosService(ILogger<ZonosService> logger, IHttpClientFactory httpClientFactory, IFileSystemService fileSystemService, IOptions<ZonosConfiguration> configuration) : IZonosService
{
    public async Task<byte[]> ProcessAsync(
        string text, string language = "en-us", string? voiceFilePath = null,
        string modelChoice = "Zyphra/Zonos-v0.1-hybrid",
        string? prefixAudioPath = null,
        float happiness = 1.0f, float sadness = 0.05f, float disgust = 0.05f, float fear = 0.05f, float surprise = 0.05f, float anger = 0.05f, float other = 0.1f, float neutral = 0.2f,
        float vqScore = 0.78f, float maxFrequency = 24000f, float pitchStd = 45.0f, float speakingRate = 15.0f, float dnsmosOverall = 4.0f, bool denoiseSpeaker = false,
        float cfgScale = 2.0f, float samplingTopP = 0.0f, int samplingTopK = 0, float samplingMinP = 0.0f, float samplingLinear = 0.5f, float samplingConfidence = 0.4f, float samplingQuadratic = 0.0f,
        int seed = 420, bool randomizeSeed = false, string[]? unconditionalKeys = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"{configuration.Value.Url}/api/generate_audio_raw";

        using var formData = new MultipartFormDataContent();

        formData.Add(new StringContent(modelChoice), "model_choice");
        formData.Add(new StringContent(text), "text");
        formData.Add(new StringContent(language), "language");

        if (!string.IsNullOrEmpty(voiceFilePath))
        {
            var bytes = await fileSystemService.ReadFileBytesAsync(voiceFilePath, cancellationToken);

            if (bytes == null)
            {
                throw new FileNotFoundException("Voice path does not exist", voiceFilePath);
            }

            var speakerAudioContent = new ByteArrayContent(bytes);
            speakerAudioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
            formData.Add(speakerAudioContent, "speaker_audio", Path.GetFileName(voiceFilePath));
        }

        if (!string.IsNullOrEmpty(prefixAudioPath))
        {
            var bytes = await fileSystemService.ReadFileBytesAsync(prefixAudioPath, cancellationToken);

            if (bytes == null)
            {
                throw new FileNotFoundException("Prefix audio path does not exist", prefixAudioPath);
            }

            var prefixAudioContent = new ByteArrayContent(bytes);
            prefixAudioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
            formData.Add(prefixAudioContent, "prefix_audio", Path.GetFileName(prefixAudioPath));
        }

        // ReSharper disable StringLiteralTypo
        formData.Add(new StringContent(happiness.ToString(CultureInfo.InvariantCulture)), "e1"); // happiness
        formData.Add(new StringContent(sadness.ToString(CultureInfo.InvariantCulture)), "e2"); // sadness
        formData.Add(new StringContent(disgust.ToString(CultureInfo.InvariantCulture)), "e3"); // disgust
        formData.Add(new StringContent(fear.ToString(CultureInfo.InvariantCulture)), "e4"); // fear
        formData.Add(new StringContent(surprise.ToString(CultureInfo.InvariantCulture)), "e5"); // surprise
        formData.Add(new StringContent(anger.ToString(CultureInfo.InvariantCulture)), "e6"); // anger
        formData.Add(new StringContent(other.ToString(CultureInfo.InvariantCulture)), "e7"); // other
        formData.Add(new StringContent(neutral.ToString(CultureInfo.InvariantCulture)), "e8"); // neutral
        formData.Add(new StringContent(vqScore.ToString(CultureInfo.InvariantCulture)), "vq_single"); // vq score
        formData.Add(new StringContent(maxFrequency.ToString(CultureInfo.InvariantCulture)), "fmax"); // max frequency (Hz)
        formData.Add(new StringContent(pitchStd.ToString(CultureInfo.InvariantCulture)), "pitch_std"); // pitch std
        formData.Add(new StringContent(speakingRate.ToString(CultureInfo.InvariantCulture)), "speaking_rate"); // speaking rate
        formData.Add(new StringContent(dnsmosOverall.ToString(CultureInfo.InvariantCulture)), "dnsmos_ovrl");
        formData.Add(new StringContent(denoiseSpeaker.ToString(CultureInfo.InvariantCulture).ToLower()), "speaker_noised"); // denoise speaker
        formData.Add(new StringContent(cfgScale.ToString(CultureInfo.InvariantCulture)), "cfg_scale"); // cfg scale
        formData.Add(new StringContent(samplingTopP.ToString(CultureInfo.InvariantCulture)), "top_p"); // top p
        formData.Add(new StringContent(samplingTopK.ToString(CultureInfo.InvariantCulture)), "top_k"); // top k
        formData.Add(new StringContent(samplingMinP.ToString(CultureInfo.InvariantCulture)), "min_p"); // min p
        formData.Add(new StringContent(samplingLinear.ToString(CultureInfo.InvariantCulture)), "linear"); // linear
        formData.Add(new StringContent(samplingConfidence.ToString(CultureInfo.InvariantCulture)), "confidence"); // confidence
        formData.Add(new StringContent(samplingQuadratic.ToString(CultureInfo.InvariantCulture)), "quadratic"); // quadratic
        formData.Add(new StringContent(seed.ToString(CultureInfo.InvariantCulture)), "seed"); // seed
        formData.Add(new StringContent(randomizeSeed.ToString(CultureInfo.InvariantCulture).ToLower()), "randomize_seed"); // random seed
        // ReSharper restore StringLiteralTypo

        var unconditionalKeysStr = unconditionalKeys is { Length: > 0 }
            ? string.Join(",", unconditionalKeys)
            : "emotion";
        formData.Add(new StringContent(unconditionalKeysStr), "unconditional_keys");

        logger.LogInformation("Processing audio for text: {text}", text);

        var httpClient = httpClientFactory.CreateClient();

        var response = await httpClient.PostAsync(url, formData, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        var size = Math.Round(result.Length / 1024.0, 2);
        logger.LogInformation("Received {bytes} byte(s) of audio data", size);

        return result;
    }
}