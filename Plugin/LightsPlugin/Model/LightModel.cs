﻿using System.ComponentModel;
using System.Text.Json.Serialization;

namespace KernelSample.Plugin.LightsPlugin.Model;
public class LightModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("is_on")]
    public bool? IsOn { get; set; }

    [JsonPropertyName("brightness")]
    public Brightness? Brightness { get; set; }

    [JsonPropertyName("color")]
    [Description("The color of the light with a hex code (ensure you include the # symbol)")]
    public string? Color { get; set; }

    [JsonPropertyName("hex")]
    public string? Hex { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Brightness
{
    Low,
    Medium,
    High
}