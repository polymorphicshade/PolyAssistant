using Microsoft.AspNetCore.Mvc;
using PolyAssistant.Core;
using PolyAssistant.Core.Models.Web;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class WebController(ISearchService searchService, IScrapeService scrapeService) : ControllerBase
{
    /// <summary>
    ///     Aggregate news articles.
    /// </summary>
    [HttpPost, Route("news")]
    public async Task<IActionResult> NewsAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Convert web pages to markdown.
    /// </summary>
    [HttpPost, Route("scrape")]
    public async Task<IActionResult> ScrapeAsync([FromBody] ScrapeQueryModel query)
    {
        var result = await scrapeService.ScrapeAsync(query.Url, query.ScrapeQueryType);

        return Ok(result);
    }

    /// <summary>
    ///     Aggregate search results from the web.
    /// </summary>
    [HttpPost, Route("search")]
    public async Task<IActionResult> SearchAsync([FromBody] SearchQueryModel query)
    {
        var queryStr = query.Query;

        switch (query.SearchQueryType)
        {
            case SearchQueryType.General:
            {
                var result =
                    await searchService
                        .QueryAsync(queryStr)
                        .ToArrayAsync();

                return Ok(result);
            }
            case SearchQueryType.News:
            {
                var result =
                    await searchService
                        .QueryNewsAsync(queryStr)
                        .ToArrayAsync();

                return Ok(result);
            }
            case SearchQueryType.Wikipedia:
            {
                var result = await searchService.QueryWikipediaAsync(queryStr);

                return Ok(result);
            }
            default:
                return BadRequest();
        }
    }
}