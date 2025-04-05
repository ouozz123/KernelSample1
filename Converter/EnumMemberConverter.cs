using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KernelSample.Converter;

/// <summary>
/// EnumMemberConverter
/// </summary>
/// <typeparam name="T"></typeparam>
/// <example>
//  [JsonConverter(typeof(EnumMemberConverter<Color>))]
//  public enum Color
//  {
//      [EnumMember(Value = "bright-red")]
//      Red,
//      [EnumMember(Value = "forest-green")]
//      Green,
//      [EnumMember(Value = "sky-blue")]
//      Blue
//  }
/// </example>
public class EnumMemberConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString();
        foreach (var field in typeToConvert.GetFields())
        {
            var attribute = field.GetCustomAttribute<EnumMemberAttribute>();
            if (attribute != null && attribute.Value == value)
            {
                return (T)Enum.Parse(typeToConvert, field.Name);
            }
        }
        return (T)Enum.Parse(typeToConvert, value, ignoreCase: true); // 回退到名稱匹配
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<EnumMemberAttribute>();
        writer.WriteStringValue(attribute?.Value ?? value.ToString());
    }
}


