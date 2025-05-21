using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Microsoft.Extensions.Logging;
using PolyAssistant.Desktop.Components;
using PolyAssistant.Desktop.Services.Interfaces;

namespace PolyAssistant.Desktop.Services;

public sealed class AutomationService(ILogger<AutomationService> logger) : IAutomationService
{
    public VisualTreeNode? GetVisualTreeFromFocusedElement()
    {
        using var automation = new UIA3Automation();

        var window = automation.FindFocusedWindow();

        return window == null
            ? null
            : GetVisualTree(window);
    }

    private static VisualTreeNode GetVisualTree(AutomationElement element)
    {
        var name = element.TryGetPropertyValue(x => x.Name.Value) ?? "NULL";
        var controlType = element.TryGetPropertyValue(x => x.ControlType.Value);
        var automationId = element.TryGetPropertyValue(x => x.AutomationId.Value) ?? "NULL";
        var className = element.TryGetPropertyValue(x => x.ClassName.Value) ?? "NULL";
        var frameworkId = element.TryGetPropertyValue(x => x.FrameworkId.Value) ?? "NULL";
        var children = element.FindAllChildren();

        var node = new VisualTreeNode(name, controlType, automationId, className, frameworkId);

        foreach (var child in children)
        {
            node.Children.Add(GetVisualTree(child));
        }

        return node;
    }
}