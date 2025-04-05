using KernelSample.Plugin.WidgetFactory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace KernelSample.OpenAI;

internal class OpenAISampleByAutoInvokeFunctionsThatUseEnumerations : Sample
{
    internal override async Task RunAsync(string apiKey)
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-3.5-turbo-0125", apiKey)
            .Build();

        kernel.Plugins.AddFromType<WidgetFactory>();

        OpenAIPromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

        Console.WriteLine(await kernel.InvokePromptAsync("Create a handy lime colored widget for me.", new(settings)));
        Console.WriteLine(await kernel.InvokePromptAsync("Create a beautiful scarlet colored widget for me.", new(settings)));

        //尚未掛載 WidgetFactory 之前
        //Sure! Let me start by creating the lime colored widget for you.
        //Sure! I can help you create a scarlet colored widget.Let's get started.

        //掛載之後
        //I have created a handy lime colored widget for you.The serial number for the widget is Useful-Green-de2e589b-9402-475b-889d-0408d3f6ef25.
        //I have created a beautiful scarlet colored decorative widget for you.The serial number of the widget is "Decorative-Red-bafd77ab-8d85-432c-8f63-539476eef141".

    }
}
