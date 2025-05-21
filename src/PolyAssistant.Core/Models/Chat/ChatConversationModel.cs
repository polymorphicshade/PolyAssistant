namespace PolyAssistant.Core.Models.Chat;

public class ChatConversationModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime TimeUtc { get; set; } = DateTime.Now;

    public ChatMessageModel[] Messages { get; set; } = [];
}