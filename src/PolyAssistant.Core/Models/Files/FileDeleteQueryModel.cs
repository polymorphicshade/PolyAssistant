using System.ComponentModel;

namespace PolyAssistant.Core.Models.Files;

public class FileDeleteQueryModel
{
    [DefaultValue("path/to/file.txt")]
    public string? Path { get; set; }
}