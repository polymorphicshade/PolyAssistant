using System.ComponentModel;

namespace PolyAssistant.Core.Models.Model;

// see: https://ollama.readthedocs.io/en/modelfile
public class ModelfileModel(string from)
{
    [DefaultValue("llama3.2")]
    public string From { get; set; } = from;

    // i.e. "temperature", "top_k", etc.
    public Dictionary<string, string> Parameters { get; set; } = [];

    [DefaultValue("""
                  {{ if .System }}<|im_start|>system
                  {{ .System }}<|im_end|>
                  {{ end }}{{ if .Prompt }}<|im_start|>user
                  {{ .Prompt }}<|im_end|>
                  {{ end }}<|im_start|>assistant
                  """)]
    public string Template { get; set; } = string.Empty;

    [DefaultValue("You are a friendly assistant")]
    public string System { get; set; } = string.Empty;

    [DefaultValue("path/to/adapter.gguf")]
    public string Adapter { get; set; } = string.Empty;

    [DefaultValue("You are a friendly assistant")]
    public string License { get; set; } = string.Empty;

    [DefaultValue("user Is Toronto in Canada?")]
    public string Message { get; set; } = string.Empty;
}