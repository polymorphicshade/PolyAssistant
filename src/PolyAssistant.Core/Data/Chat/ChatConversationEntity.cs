namespace PolyAssistant.Core.Data.Chat;

public sealed class ChatConversationEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime TimeUtc { get; set; } = DateTime.Now;

    public ICollection<ChatMessageEntity> Messages { get; set; } = [];
}