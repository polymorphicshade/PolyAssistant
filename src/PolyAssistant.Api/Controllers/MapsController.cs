using Microsoft.AspNetCore.Mvc;

namespace PolyAssistant.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class MapsController : ControllerBase
{
    /// <summary>
    ///     Get directions based off a geolocation.
    /// </summary>
    [HttpPost, Route("directions")]
    public Task<IActionResult> DirectionsAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Query information one would find using Google Maps.
    /// </summary>
    [HttpPost, Route("query")]
    public Task<IActionResult> QueryAsync()
    {
        throw new NotImplementedException();
    }
}