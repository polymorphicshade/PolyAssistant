namespace PolyAssistant.Core.Models.Time;

public sealed class TimeZoneInfoModel
{
    public string Id { get; set; } = string.Empty;

    public TimeSpan Offset { get; set; }
}