using System.ComponentModel;

namespace PolyAssistant.Core.Models.Web;

public class ScrapeQueryModel(string url, ScrapeQueryType scrapeQueryType)
{
    [DefaultValue("http://example.com")]
    public string Url { get; } = url;

    [DefaultValue(ScrapeQueryType.Markdown)]
    public ScrapeQueryType ScrapeQueryType { get; } = scrapeQueryType;
}