using Microsoft.SemanticKernel.ChatCompletion;

namespace PolyAssistant.Core.Models.Chat;

public sealed class ChatMessageModel
{
    public long Id { get; set; }

    public DateTime TimeUtc { get; set; } = DateTime.Now;

    public string Role { get; set; } = AuthorRole.User.Label;

    public string Content { get; set; } = string.Empty;
}