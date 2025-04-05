using KernelSample.Plugin.LightsPlugin;
using KernelSample.Plugin.LightsPlugin.Model;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace KernelSample.OpenAI;
internal class OpenAISampleByPlugin : Sample
{
    internal override async Task RunAsync(string apiKey)
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-3.5-turbo-0125", apiKey)
            .Build();

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();


        List<LightModel> lights = new()
       {
          new LightModel { Id = 1, Name = "Table Lamp", IsOn = false, Brightness = Brightness.Medium, Color = "#FFFFFF" },
          new LightModel { Id = 2, Name = "Porch light", IsOn = false, Brightness = Brightness.High, Color = "#FF0000" },
          new LightModel { Id = 3, Name = "Chandelier", IsOn = true, Brightness = Brightness.Low, Color = "#FFFF00" }
       };

        kernel.Plugins.AddFromObject(new LightsPlugin(lights));

        // Enable planning
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        // Create a history store the conversation
        var history = new ChatHistory();
        //history.AddUserMessage("Please turn on the Porch light");

        while (true)
        {
            history.AddUserMessage(Console.ReadLine()!);

            // Get the response from the AI
            var result = await chatCompletionService.GetChatMessageContentAsync(
               history,
               executionSettings: openAIPromptExecutionSettings,
               kernel: kernel);

            // Print the results
            Console.WriteLine("Assistant > " + result.Content!);

            // Add the message from the agent to the chat history
            history.AddAssistantMessage(result.Content!);
        }
    }
}
