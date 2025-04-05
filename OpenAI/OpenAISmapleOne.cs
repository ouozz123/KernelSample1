using Microsoft.SemanticKernel;

namespace KernelSample.OpenAI;

internal class OpenAISmapleOne : Sample
{
    internal override async Task RunAsync(string apiKey)
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-4o-mini", apiKey)
            .Build();

        // Q&A loop
        while (true)
        {
            Console.Write("Question: ");
            Console.WriteLine(await kernel.InvokePromptAsync(Console.ReadLine()!));
            Console.WriteLine();
        }
    }
}
