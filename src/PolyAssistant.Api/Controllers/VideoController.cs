using Microsoft.AspNetCore.Mvc;

namespace PolyAssistant.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class VideoController : ControllerBase
{
    /// <summary>
    ///     Extend a video.
    /// </summary>
    [HttpPost, Route("extend")]
    public Task<IActionResult> ExtendAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Generate a video based on an image.
    /// </summary>
    [HttpPost, Route("image2video")]
    public Task<IActionResult> ImageToVideoAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Ask AI about a video.
    /// </summary>
    [HttpPost, Route("query")]
    public Task<IActionResult> QueryAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Slow down a video.
    /// </summary>
    [HttpPost, Route("slow")]
    public Task<IActionResult> SlowAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Generate a video based on text.
    /// </summary>
    [HttpPost, Route("text2video")]
    public Task<IActionResult> TextToVideoAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Generate a video based on another video.
    /// </summary>
    [HttpPost, Route("video2video")]
    public Task<IActionResult> VideoToVideoAsync()
    {
        throw new NotImplementedException();
    }
}