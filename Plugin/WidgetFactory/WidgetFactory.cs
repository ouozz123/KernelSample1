using KernelSample.Extensions;
using KernelSample.Plugin.WidgetFactory.Model;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace KernelSample.Plugin.WidgetFactory;

/// <summary>
/// A plugin that creates widgets.
/// </summary>
public class WidgetFactory
{
    [KernelFunction]
    [Description("Creates a new widget of the specified type and colors")]
    public WidgetDetails CreateWidget([Description("The type of widget to be created")] WidgetType widgetType, [Description("The colors of the widget to be created")] WidgetColor[] widgetColors)
    {
        var colors = string.Join('-', widgetColors.Select(c => c.GetDisplayName()).ToArray());
        return new()
        {
            SerialNumber = $"{widgetType}-{colors}-{Guid.NewGuid()}",
            Type = widgetType,
            Colors = widgetColors
        };
    }
}