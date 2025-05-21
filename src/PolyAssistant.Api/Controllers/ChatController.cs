using Microsoft.AspNetCore.Mvc;
using PolyAssistant.Core;
using PolyAssistant.Core.Models.Chat;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class ChatController(IOllamaService ollamaService) : ControllerBase
{
    /// <summary>
    ///     Clears conversation(s) from the cache.
    /// </summary>
    [HttpPost, Route("clear")]
    public async Task<IActionResult> ClearAsync([FromBody] Guid[]? conversationIds = null)
    {
        await ollamaService.ClearConversationsAsync(conversationIds);

        return Ok();
    }

    /// <summary>
    ///     Get conversation(s) from the cache.
    /// </summary>
    [HttpPost, Route("find")]
    public IActionResult Find([FromBody] ChatFindQueryModel query)
    {
        if (query.Ids is { Length: > 0 })
        {
            var result =
                ollamaService
                    .GetConversationsAsync(query.Ids)
                    .ToArray();

            return Ok(result);
        }
        else
        {
            var result =
                ollamaService
                    .GetConversationsAsync(
                        query.ConversationTopCount,
                        query.MessagesTopCount,
                        query.ConversationFrom?.ToDateTime(),
                        query.ConversationTo?.ToDateTime(),
                        query.MessageFrom?.ToDateTime(),
                        query.MessageTo?.ToDateTime())
                    .ToArray();

            return Ok(result);
        }
    }

    /// <summary>
    ///     Send a message to AI and get a response.
    /// </summary>
    [HttpPost, Route("send")]
    public async Task<IActionResult> SendAsync([FromBody] ChatQueryModel query)
    {
        var message = query.Message;

        if (string.IsNullOrWhiteSpace(message))
        {
            return NoContent();
        }

        var systemMessage = query.SystemMessage;
        var model = query.Model;
        var conversationId = query.ConversationId;
        var isAtomic = query.IsAtomic;

        // TODO: add OpenAI prompt settings
        //
        //

        var result = await ollamaService.ChatAsync(message, systemMessage, model, null, null, conversationId, isAtomic);

        return Ok(result);
    }

    /// <summary>
    ///     Reset a conversation to the specified message.
    /// </summary>
    [HttpPost, Route("reset")]
    public async Task<IActionResult> ResetAsync([FromBody] ChatResetQueryModel query)
    {
        var result = await ollamaService.ResetConversationAsync(query.ConversationId, query.MessageId);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}