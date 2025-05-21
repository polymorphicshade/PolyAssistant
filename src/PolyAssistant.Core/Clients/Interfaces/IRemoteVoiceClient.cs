using PolyAssistant.Core.Models.Voice;

namespace PolyAssistant.Core.Clients.Interfaces;

public interface IRemoteVoiceClient
{
    string Url { get; }

    Task ChangeAsync(CancellationToken cancellationToken = default);

    Task<byte[]> GenerateAsync(VoiceGenerationQueryModel query, CancellationToken cancellationToken = default);

    Task<byte[]> GenerateExAsync(VoiceGenerationExQueryModel query, CancellationToken cancellationToken = default);
}