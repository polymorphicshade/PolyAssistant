using PolyAssistant.Core.Models.Time;

namespace PolyAssistant.Core.Clients.Interfaces;

public interface IRemoteTimeClient
{
    string Url { get; }

    Task<string> GetCurrentTimeAsync(string? timeZone = null, string? stringFormat = null, CancellationToken cancellationToken = default);

    Task<TimeZoneInfoModel[]> GetTimeZonesAsync(CancellationToken cancellationToken = default);
}