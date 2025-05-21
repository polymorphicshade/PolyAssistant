using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Services;

// see: https://docs.crawl4ai.com/
public sealed class ScrapeService(ILogger<ScrapeService> logger, IHttpClientFactory httpClientFactory, IOptions<Crawl4AiConfiguration> configuration) : IScrapeService
{
    public async Task<string?> ScrapeAsync(string url, ScrapeQueryType scrapeQueryType, CancellationToken cancellationToken = default)
    {
        return scrapeQueryType switch
        {
            ScrapeQueryType.Html => await ScrapeAsHtmlAsync(url, cancellationToken),
            ScrapeQueryType.Markdown => await ScrapeAsMarkdownAsync(url, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(scrapeQueryType), scrapeQueryType, null)
        };
    }

    private async Task<string?> ScrapeAsHtmlAsync(string url, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{configuration.Value.Url}/crawl");

        logger.LogInformation("Scraping HTML: {url}", url);

        var httpClient = httpClientFactory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        // TODO: do better
        var jsonContent = JsonSerializer.Serialize(new
        {
            urls = new[]
            {
                url
            },
            crawler_config = new
            {
                type = "CrawlerRunConfig",
                @params = new
                {
                    scraping_strategy = new
                    {
                        type = "WebScrapingStrategy",
                        @params = new { }
                    },
                    exclude_social_media_domains = new[]
                    {
                        "facebook.com",
                        "twitter.com",
                        "x.com",
                        "linkedin.com",
                        "instagram.com",
                        "pinterest.com",
                        "tiktok.com",
                        "snapchat.com",
                        "reddit.com"
                    },
                    stream = false
                }
            }
        });

        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        var obj = JsonSerializer.Deserialize<JsonObject>(json);

        if (obj == null)
        {
            return null;
        }

        var success = obj["success"]?.GetValue<bool>();

        if (success != true)
        {
            // TODO: log details
            throw new HttpRequestException("Internal crawling resulted in an error");
        }

        return obj["results"]?[0]?["cleaned_html"]?.GetValue<string>();
    }

    private async Task<string?> ScrapeAsMarkdownAsync(string url, CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{configuration.Value.Url}/md");

        logger.LogInformation("Scraping Markdown: {url}", url);

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);

        var httpClient = httpClientFactory.CreateClient();

        // TODO: do better
        var jsonContent = JsonSerializer.Serialize(new
        {
            url,
            f = "fit",
            q = "null",
            c = "0"
        });

        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        var obj = JsonSerializer.Deserialize<JsonObject>(json);

        if (obj == null)
        {
            return null;
        }

        var success = obj["success"]?.GetValue<bool>();

        if (success != true)
        {
            // TODO: log details
            throw new HttpRequestException("Internal crawling request resulted in an error");
        }

        var result = obj["markdown"]?.GetValue<string>();

        result =
            result?
                .Replace("\n", Environment.NewLine)
                .Replace("\r", Environment.NewLine);

        return result;
    }
}