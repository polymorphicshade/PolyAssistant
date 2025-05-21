using System.ComponentModel;

namespace PolyAssistant.Core.Models.Chat;

public sealed class ChatFindQueryModel
{
    [DefaultValue(null)]
    public Guid[]? Ids { get; set; }

    [DefaultValue(5)]
    public int ConversationTopCount { get; set; } = 5;

    [DefaultValue(10)]
    public int MessagesTopCount { get; set; } = 10;

    [DefaultValue("5/1/2025")]
    public string? ConversationFrom { get; set; } = null;

    [DefaultValue("5/25/2025")]
    public string? ConversationTo { get; set; } = null;

    [DefaultValue("5/1/2025")]
    public string? MessageFrom { get; set; } = null;

    [DefaultValue("5/25/2025")]
    public string? MessageTo { get; set; } = null;
}