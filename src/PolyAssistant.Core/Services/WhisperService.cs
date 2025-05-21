using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Services;

public sealed class WhisperService(ILogger<WhisperService> logger, IHttpClientFactory httpClientFactory, IOptions<WhisperConfiguration> configuration) : IWhisperService
{
    public async Task<string> TranscribeAsync(byte[] wavData, string? voice = null, string? language = null, CancellationToken cancellationToken = default)
    {
        var url = $"{configuration.Value.Url}/asr";

        var httpClient = httpClientFactory.CreateClient();

        var byteContent = new ByteArrayContent(wavData);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

        using MultipartFormDataContent formData = new();
        formData.Add(byteContent, "audio_file", "audio.wav");

        using var response = await httpClient.PostAsync(url, formData, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync(cancellationToken);

        result =
            result
                .Replace(Environment.NewLine, " ")
                .Replace("\n", " ")
                .Trim();

        if (string.IsNullOrWhiteSpace(result))
        {
            logger.LogInformation("Transcribed audio: <none>");
        }
        else
        {
            logger.LogInformation("Transcribed audio: {text}", result);
        }

        return result;
    }
}