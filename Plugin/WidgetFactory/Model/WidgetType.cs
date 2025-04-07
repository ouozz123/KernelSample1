using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace KernelSample.Plugin.WidgetFactory.Model;

/// <summary>
/// 請參考 <see cref="JsonConverter"/> 並取得 <see cref="EnumMember"/> 的 Value
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
//[JsonConverter(typeof(EnumMemberConverter<WidgetType>))]
public enum WidgetType
{
    //[Description("A widget that is useful.")]
    [EnumMember(Value = "一個有用的小部件")]
    Useful,

    //[Description("A widget that is decorative.")]
    [EnumMember(Value = "裝飾性的小部件")]
    Decorative
}