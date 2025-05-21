using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyAssistant.Core.Clients;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Models.Web;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Services;

public sealed class SearchService(ILogger<SearchService> logger, IHttpClientFactory httpClientFactory, IOptions<SearXngConfiguration> configuration) : ISearchService
{
    private readonly SearXngClient _client = new(
        configuration.Value.Url,
        httpClientFactory.CreateClient(),
        logger);

    public async IAsyncEnumerable<SearchResultModel> QueryAsync(string query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var result = await _client.SearchAsync(query, cancellationToken: cancellationToken);

        if (result == null)
        {
            throw new NullReferenceException("Client search resulted in a NULL value (this shouldn't happen)");
        }

        foreach (var item in result["results"]?.AsArray() ?? [])
        {
            if (item == null)
            {
                continue;
            }

            var url = item["url"]?.GetValue<string>();
            var title = item["title"]?.GetValue<string>();
            var content = item["content"]?.GetValue<string>();
            var score = item["score"]?.GetValue<double>();
            var category = item["category"]?.GetValue<string>();
            var publishedDate = item["publishedDate"]?.GetValue<DateTime>();

            if (string.IsNullOrWhiteSpace(url)
                || string.IsNullOrWhiteSpace(title)
                || string.IsNullOrWhiteSpace(content)
                || score == null
                || string.IsNullOrWhiteSpace(category))
            {
                continue;
            }

            yield return new SearchResultModel(url, title, content, score.Value, category, publishedDate);
        }
    }

    public async IAsyncEnumerable<SearchResultModel> QueryNewsAsync(string query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var result = await _client.SearchAsync(query, "!news", cancellationToken);

        if (result == null)
        {
            throw new NullReferenceException("Client search resulted in a NULL value (this shouldn't happen)");
        }

        foreach (var item in result["results"]?.AsArray() ?? [])
        {
            if (item == null)
            {
                continue;
            }

            var url = item["url"]?.GetValue<string>();
            var title = item["title"]?.GetValue<string>();
            var content = item["content"]?.GetValue<string>();
            var score = item["score"]?.GetValue<double>();
            var category = item["category"]?.GetValue<string>();
            var publishedDate = item["publishedDate"]?.GetValue<DateTime>();

            if (string.IsNullOrWhiteSpace(url)
                || string.IsNullOrWhiteSpace(title)
                || string.IsNullOrWhiteSpace(content)
                || score == null
                || string.IsNullOrWhiteSpace(category))
            {
                continue;
            }

            yield return new SearchResultModel(url, title, content, score.Value, category, publishedDate);
        }
    }

    public async Task<Uri?> QueryWikipediaAsync(string query, CancellationToken cancellationToken = default)
    {
        var result = await _client.SearchAsync(query, "!wp", cancellationToken);

        if (result == null)
        {
            throw new NullReferenceException("Client search resulted in a NULL value (this shouldn't happen)");
        }

        // ReSharper disable once StringLiteralTypo
        var target = result["infoboxes"]?[0];

        var url = target?["id"]?.GetValue<string>();

        return url == null
            ? null
            : new Uri(url);
    }

    // TODO: add "!re" (reddit search)
    //
    //
}