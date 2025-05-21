using System.ComponentModel;

namespace PolyAssistant.Core.Models.Chat;

public sealed class ChatQueryModel
{
    [DefaultValue("Why is the sky blue?")]
    public string? Message { get; set; }

    [DefaultValue("You are a helpful assistant. Keep your responses brief.")]
    public string? SystemMessage { get; set; }

    [DefaultValue("llama3.2:latest")]
    public string? Model { get; set; }

    [DefaultValue(null)]
    public Guid? ConversationId { get; set; }

    [DefaultValue(true)]
    public bool IsAtomic { get; set; }
}