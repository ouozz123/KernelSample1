using System.ComponentModel;
using System.Text.Json.Serialization;

namespace KernelSample.Plugin.WidgetFactory.Model;

/// <summary>
/// A <see cref="JsonConverter"/> is required to correctly convert enum values.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WidgetColor
{
    [Description("Use when creating a red item.")]
    Red,

    [Description("Use when creating a green item.")]
    Green,

    [Description("Use when creating a blue item.")]
    Blue
}