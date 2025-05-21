using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Services.Interfaces;

// ReSharper disable StringLiteralTypo

namespace PolyAssistant.Core.Services;

public sealed class ChatterboxService(ILogger<ChatterboxService> logger, IHttpClientFactory httpClientFactory, IFileSystemService fileSystemService, IOptions<ChatterboxConfiguration> configuration) : IChatterboxService
{
    public async Task<byte[]> ProcessAsync(
        string text,
        string? voiceFilePath = null,
        int? seed = null,
        double? exaggeration = 0.5, double? pace = 0.5, double? temperature = 0.8,
        CancellationToken cancellationToken = default)
    {
        var url = $"{configuration.Value.Url}/api/generate";

        using var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(text), "text");
        formData.Add(new StringContent($"{seed ?? 0}"), "seed");
        formData.Add(new StringContent($"{exaggeration ?? 0.5}"), "exaggeration");
        formData.Add(new StringContent($"{temperature ?? 0.8}"), "temperature");
        formData.Add(new StringContent($"{pace ?? 0.5}"), "cfgw");

        if (!string.IsNullOrEmpty(voiceFilePath))
        {
            var bytes = await fileSystemService.ReadFileBytesAsync(voiceFilePath, cancellationToken);

            if (bytes == null)
            {
                throw new FileNotFoundException("Voice path does not exist", voiceFilePath);
            }

            var voiceDataContent = new ByteArrayContent(bytes);
            voiceDataContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
            formData.Add(voiceDataContent, "voice_data", Path.GetFileName(voiceFilePath));
        }

        var httpClient = httpClientFactory.CreateClient();

        var response = await httpClient.PostAsync(url, formData, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        var size = Math.Round(result.Length / 1024.0, 2);
        logger.LogInformation("Received {bytes} byte(s) of audio data", size);

        return result;
    }
}