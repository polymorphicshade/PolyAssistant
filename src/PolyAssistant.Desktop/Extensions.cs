using System.IO;
using System.Windows.Media.Imaging;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Microsoft.Extensions.DependencyInjection;
using PolyAssistant.Core.Agents.Interfaces;
using PolyAssistant.Desktop.Components.Interfaces;
using PolyAssistant.Desktop.Models;
using Application = System.Windows.Application;
using AutomationElement = FlaUI.Core.AutomationElements.AutomationElement;
using ControlType = FlaUI.Core.Definitions.ControlType;
using Window = FlaUI.Core.AutomationElements.Window;

namespace PolyAssistant.Desktop;

public static class Extensions
{
    public static T GetOrCreateInstance<T>(this Application? application)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (application is not IApplicationServiceProvider applicationServiceProvider)
        {
            return Activator.CreateInstance<T>();
        }

        var provider = applicationServiceProvider.ServiceProvider;
        return ActivatorUtilities.GetServiceOrCreateInstance<T>(provider);
    }

    public static AgentModel ToModel(this IAgent agent)
    {
        return new AgentModel(agent);
    }

    public static AudioOutputDeviceModel ToModel(this IAudioOutputDevice device)
    {
        return new AudioOutputDeviceModel(device);
    }

    public static AudioInputDeviceModel ToModel(this IAudioInputDevice device)
    {
        return new AudioInputDeviceModel(device);
    }

    public static byte[] ToBytes(this BitmapSource bitmapSource, BitmapEncoder encoder)
    {
        using var stream = new MemoryStream();
        var frame = BitmapFrame.Create(bitmapSource);
        encoder.Frames.Add(frame);
        encoder.Save(stream);

        return stream.ToArray();
    }

    // automation

    public static IEnumerable<Window> FindWindows(this UIA3Automation automation)
    {
        return
            automation
                .GetDesktop()
                .FindAllChildren(x => x.ByControlType(ControlType.Window))
                .Select(x => x.AsWindow());
    }

    public static Window? FindFocusedWindow(this UIA3Automation automation)
    {
        var focusedElement = automation.FocusedElement();

        var currentElement = focusedElement;

        while (currentElement.ControlType != ControlType.Window)
        {
            currentElement = currentElement.Parent;
        }

        return currentElement.ControlType == ControlType.Window
            ? currentElement.AsWindow()
            : null;
    }

    public static AutomationElement? FindFocusedEditElement(this UIA3Automation automation)
    {
        var element = automation.FocusedElement();

        return element.IsTextEditable()
            ? element
            : null;
    }

    public static bool IsTextEditable(this AutomationElement element)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch (element.ControlType)
        {
            case ControlType.Edit:
            case ControlType.ComboBox:
            case ControlType.Document:
            {
                return !element.Patterns.Value.Pattern.IsReadOnly.Value;
            }
        }

        return false;
    }

    public static void Resize(this Window window, double newWidth, double newHeight)
    {
        var transformPattern = window.Patterns.Transform.Pattern;

        if (transformPattern.CanResize.Value)
        {
            transformPattern.Resize(newWidth, newHeight);
        }
    }

    public static T? TryGetPropertyValue<T>(this AutomationElement element, Func<FrameworkAutomationElementBase.IProperties, T> selector, T? defaultValue = default)
    {
        try
        {
            return selector(element.Properties);
        }
        catch
        {
            return defaultValue;
        }
    }
}