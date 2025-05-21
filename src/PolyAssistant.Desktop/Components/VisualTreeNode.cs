using FlaUI.Core.Definitions;

namespace PolyAssistant.Desktop.Components;

public sealed class VisualTreeNode(string name, ControlType controlType, string automationId, string className, string frameworkId)
{
    public string Name { get; } = name;

    public ControlType ControlType { get; } = controlType;

    public string AutomationId { get; } = automationId;

    public string ClassName { get; } = className;

    public string FrameworkId { get; } = frameworkId;

    public List<VisualTreeNode> Children { get; set; } = [];
}