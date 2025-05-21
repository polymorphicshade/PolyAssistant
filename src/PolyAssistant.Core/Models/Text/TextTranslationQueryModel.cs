using System.ComponentModel;

namespace PolyAssistant.Core.Models.Text;

public sealed class TextTranslationQueryModel
{
    [DefaultValue("Where can I find the bathroom?")]
    public string? Text { get; set; }

    [DefaultValue("Japanese")]
    public string? To { get; set; }

    [DefaultValue("English")]
    public string? From { get; set; }

    [DefaultValue("0.9")]
    public double Consistency { get; set; } = 0.9;
}