using PolyAssistant.Desktop.Components;

namespace PolyAssistant.Desktop.Services.Interfaces;

public interface IAutomationService
{
    VisualTreeNode? GetVisualTreeFromFocusedElement();
}