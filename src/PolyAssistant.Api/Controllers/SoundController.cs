using Microsoft.AspNetCore.Mvc;

namespace PolyAssistant.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class SoundController : ControllerBase
{
    /// <summary>
    ///     Generate audio based on another audio sample.
    /// </summary>
    [HttpPost, Route("audio2audio")]
    public Task<IActionResult> AudioToAudioAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Generate audio based on text.
    /// </summary>
    [HttpPost, Route("text2audio")]
    public Task<IActionResult> TextToAudioAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Transfer audio to another.
    /// </summary>
    [HttpPost, Route("transfer")]
    public Task<IActionResult> TransferAsync()
    {
        throw new NotImplementedException();
    }
}