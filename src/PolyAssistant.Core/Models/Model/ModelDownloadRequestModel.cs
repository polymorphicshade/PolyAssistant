using System.ComponentModel;

namespace PolyAssistant.Core.Models.Model;

public class ModelDownloadRequestModel(string url)
{
    [DefaultValue("https://domain.com/path/to/model.gguf")]
    public string Url { get; set; } = url;

    [DefaultValue("path/to/save/model.gguf")]
    public string Path { get; set; } = string.Empty;
}