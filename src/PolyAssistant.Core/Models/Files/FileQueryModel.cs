using System.ComponentModel;

namespace PolyAssistant.Core.Models.Files;

public class FileQueryModel
{
    [DefaultValue(".*")]
    public string? Pattern { get; set; } = ".*";
}