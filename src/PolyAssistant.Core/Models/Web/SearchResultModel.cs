namespace PolyAssistant.Core.Models.Web;

public class SearchResultModel(string url, string title, string content, double score, string category, DateTime? publishedDate)
{
    public string Url { get; } = url;

    public string Title { get; } = title;

    public string Content { get; } = content;

    public double Score { get; } = score;

    public string Category { get; } = category;

    public DateTime? PublishedDate { get; } = publishedDate;
}