using Microsoft.AspNetCore.Mvc;
using PolyAssistant.Core;
using PolyAssistant.Core.Models.Time;

namespace PolyAssistant.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class TimeController : ControllerBase
{
    /// <summary>
    ///     Gets the current time.
    /// </summary>
    [HttpGet, Route("current")]
    public IActionResult GetCurrentTime(string? timeZone = null, string? stringFormat = null)
    {
        var target = DateTime.UtcNow;

        if (timeZone != null)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

                // see: https://timeapi.io/documentation/iana-timezones
                target = TimeZoneInfo.ConvertTimeFromUtc(target, tz);
            }
            catch (TimeZoneNotFoundException)
            {
                return NotFound($"Timezone not found: {timeZone}");
            }
        }

        var timeUtcMilliseconds = target.GetUtcMilliseconds();

        return string.IsNullOrWhiteSpace(stringFormat)
            ? Ok(timeUtcMilliseconds)
            : Ok(target.ToString(stringFormat));
    }

    /// <summary>
    ///     Gets all the timezones supported by the system.
    /// </summary>
    [HttpGet, Route("zones")]
    public IActionResult GetTimeZones()
    {
        var result =
            TimeZoneInfo
                .GetSystemTimeZones()
                .Select(x => new TimeZoneInfoModel
                {
                    Id = x.Id,
                    Offset = x.BaseUtcOffset
                })
                .ToArray();

        return Ok(result);
    }
}