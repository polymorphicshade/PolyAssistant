using System.ComponentModel;

namespace PolyAssistant.Core.Models.Model;

public class ModelCreateRequestModel(string name, ModelfileModel modelfile)
{
    [DefaultValue("My Model")]
    public string Name { get; set; } = name;

    public ModelfileModel Modelfile { get; set; } = modelfile;
}