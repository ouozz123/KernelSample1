using KernelSample.Plugin.LightsPlugin.Model;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace KernelSample.Plugin.LightsPlugin;
public class LightsPlugin
{
    private readonly List<LightModel> _lights;

    public LightsPlugin(List<LightModel> lights)
    {
        _lights = lights;
    }

    [KernelFunction("get_lights")]
    [Description("Gets a list of lights and their current state")]
    public async Task<List<LightModel>> GetLightsAsync()
    {
        return _lights;
    }

    [KernelFunction("change_state")]
    [Description("Changes the state of the light")]
    public async Task<LightModel?> ChangeStateAsync(LightModel changeState)
    {
        // Find the light to change
        var light = _lights.FirstOrDefault(l => l.Id == changeState.Id);

        // If the light does not exist, return null
        if (light == null)
        {
            return null;
        }

        // Update the light state
        light.IsOn = changeState.IsOn;
        light.Brightness = changeState.Brightness;
        light.Color = changeState.Color;

        // 將 model 序列化成 JSON 字串
        Console.WriteLine("ChangeState: {0}", JsonSerializer.Serialize(light));

        return light;
    }
}