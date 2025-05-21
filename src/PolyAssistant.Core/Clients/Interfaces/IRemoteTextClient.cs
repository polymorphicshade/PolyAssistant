using PolyAssistant.Core.Models.Text;

namespace PolyAssistant.Core.Clients.Interfaces;

public interface IRemoteTextClient
{
    string Url { get; }
    
    Task OcrAsync(CancellationToken cancellationToken = default);
    
    Task<string?> TranscribeAsync(string wavFilePath, CancellationToken cancellationToken = default);
    
    Task<string?> TranscribeAsync(byte[] wavData, CancellationToken cancellationToken = default);
    
    Task<string?> TranscribeAsync(Stream wavStream, CancellationToken cancellationToken = default);
    
    Task<string?> TranslateAsync(TextTranslationQueryModel query, CancellationToken cancellationToken = default);
}