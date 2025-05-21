namespace PolyAssistant.Core.Services.Interfaces;

public interface IChatterboxService
{
    Task<byte[]> ProcessAsync(
        string text,
        string? voiceFilePath = null,
        int? seed = null,
        double? exaggeration = 0.5, double? pace = 0.5, double? temperature = 0.8,
        CancellationToken cancellationToken = default);
}