using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Services;

namespace PolyAssistant.Core.Tests;

[TestClass]
public sealed class Sandbox
{
    [TestMethod]
    public async Task Test01()
    {
        const string url = "https://192.168.1.187/searxng/";

        var serviceProvider =
            new ServiceCollection()
                .AddInsecureHttpsClient()
                .BuildServiceProvider();

        var logger = NullLogger<SearchService>.Instance;
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var options = Options.Create(new SearXngConfiguration
        {
            Url = url
        });

        var service = new SearchService(logger, httpClientFactory, options);

        var result = await service.QueryNewsAsync("google").ToArrayAsync();
    }
}