namespace PolyAssistant.Core.Models.Chat;

public sealed class ChatResetQueryModel
{
    public Guid ConversationId { get; set; }

    public long? MessageId { get; set; }
}