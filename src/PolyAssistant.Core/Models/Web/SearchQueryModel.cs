using System.ComponentModel;

namespace PolyAssistant.Core.Models.Web;

public class SearchQueryModel(string query, SearchQueryType searchQueryType)
{
    [DefaultValue("string theory")]
    public string Query { get; } = query;

    [DefaultValue(SearchQueryType.Wikipedia)]
    public SearchQueryType SearchQueryType { get; } = searchQueryType;
}