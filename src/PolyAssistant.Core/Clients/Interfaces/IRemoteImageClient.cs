using PolyAssistant.Core.Models.Chat;
using PolyAssistant.Core.Models.Image;

namespace PolyAssistant.Core.Clients.Interfaces;

public interface IRemoteImageClient
{
    string Url { get; }

    Task<ChatResponseModel> QueryAsync(ImageQueryModel query, CancellationToken cancellationToken = default);

    Task<string[]> TagAsync(ImageTagQueryModel query, CancellationToken cancellationToken = default);
}