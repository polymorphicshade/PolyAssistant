namespace PolyAssistant.Core.Models.Chat;

public sealed class ChatResponseModel
{
    public DateTime TimeUtc { get; set; } = DateTime.Now;

    public string? Message { get; set; }

    public Guid? ConversationId { get; set; }
}