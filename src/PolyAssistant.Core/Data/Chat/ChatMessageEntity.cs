using System.ComponentModel.DataAnnotations;
using Microsoft.SemanticKernel.ChatCompletion;

namespace PolyAssistant.Core.Data.Chat;

public sealed class ChatMessageEntity
{
    [Key]
    public long Id { get; set; }

    public DateTime TimeUtc { get; set; } = DateTime.Now;

    [MaxLength(256)]
    public string Role { get; set; } = AuthorRole.User.Label;

    public string Content { get; set; } = string.Empty;

    // navigation properties

    public Guid ConversationId { get; set; }

    public ChatConversationEntity Conversation { get; set; } = new();
}