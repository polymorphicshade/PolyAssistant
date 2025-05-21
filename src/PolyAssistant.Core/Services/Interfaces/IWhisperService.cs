namespace PolyAssistant.Core.Services.Interfaces;

public interface IWhisperService
{
    Task<string> TranscribeAsync(byte[] wavData, string? voice = null, string? language = null, CancellationToken cancellationToken = default);
}