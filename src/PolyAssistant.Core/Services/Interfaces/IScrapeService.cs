namespace PolyAssistant.Core.Services.Interfaces;

public interface IScrapeService
{
    Task<string?> ScrapeAsync(string url, ScrapeQueryType scrapeQueryType, CancellationToken cancellationToken = default);
}