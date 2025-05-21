using Microsoft.AspNetCore.Mvc;
using PolyAssistant.Core.Models.Image;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class ImageController(IOllamaService ollamaService) : ControllerBase
{
    /// <summary>
    ///     Generate an image based on another image.
    /// </summary>
    [HttpPost, Route("image2image")]
    public Task<IActionResult> ImageToImageAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Ask AI about an image.
    /// </summary>
    [HttpPost, Route("query")]
    public async Task<IActionResult> QueryAsync([FromBody] ImageQueryModel query)
    {
        var message = query.Message;

        if (string.IsNullOrWhiteSpace(message))
        {
            return BadRequest("Message cannot be empty");
        }

        var imageBase64 = query.Image;

        if (string.IsNullOrWhiteSpace(imageBase64))
        {
            return BadRequest("Image string cannot be empty");
        }

        var systemMessage = query.SystemMessage;
        var model = query.Model;
        var conversationId = query.ConversationId;
        var isAtomic = query.IsAtomic;

        // TODO: add OpenAI prompt settings
        //
        //

        var result = await ollamaService.ChatAsync(message, systemMessage, model, imageBase64, null, conversationId, isAtomic);

        return Ok(result);
    }

    /// <summary>
    ///     Get tags for an image (useful for galleries and such).
    /// </summary>
    [HttpPost, Route("tag")]
    public async Task<IActionResult> TagAsync([FromBody] ImageTagQueryModel query)
    {
        var imageBase64 = query.Image;

        if (string.IsNullOrWhiteSpace(imageBase64))
        {
            return BadRequest("Image string cannot be empty");
        }

        var count = query.Count;
        var context = query.Context ?? "Use only 1-word tags, all lower-case.";
        var systemMessage = query.SystemMessage;
        var model = query.Model;
        var conversationId = query.ConversationId;
        var isAtomic = query.IsAtomic;

        var message = $"Come up with {count} image tags for the provided image." +
                      $"{Environment.NewLine}Respond in plain CSV format. Do not embellish your response." +
                      $"{Environment.NewLine}{context}";

        // TODO: add OpenAI prompt settings
        //
        //

        var response = await ollamaService.ChatAsync(message, systemMessage, model, imageBase64, null, conversationId, isAtomic);

        var csv = response.Message;

        var result =
            csv?
                .Split(',')
                .Select(x => x.ToLowerInvariant().Trim())
                .ToArray() ?? [];

        return Ok(result);
    }

    /// <summary>
    ///     Generate an image based on text.
    /// </summary>
    [HttpPost, Route("text2image")]
    public Task<IActionResult> TextToImageAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Upscale an image.
    /// </summary>
    [HttpPost, Route("upscale")]
    public Task<IActionResult> UpscaleAsync()
    {
        throw new NotImplementedException();
    }
}