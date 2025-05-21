using PolyAssistant.Core.Models.Web;

namespace PolyAssistant.Core.Services.Interfaces;

public interface ISearchService
{
    IAsyncEnumerable<SearchResultModel> QueryAsync(string query, CancellationToken cancellationToken = default);

    IAsyncEnumerable<SearchResultModel> QueryNewsAsync(string query, CancellationToken cancellationToken = default);

    Task<Uri?> QueryWikipediaAsync(string query, CancellationToken cancellationToken = default);
}