using Microsoft.SemanticKernel;

namespace KernelSample.OpenAI;
internal class OpenAISampleTwo : Sample
{
    public OpenAISampleTwo()
    {
    }

    internal override async Task RunAsync(string apiKey)
    {
        // Initialize the kernel
        Kernel kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-3.5-turbo-0125", apiKey)
            .Build();

        // Create the prompt function as part of a plugin and add it to the kernel.
        // These operations can be done separately, but helpers also enable doing
        // them in one step.
        kernel.ImportPluginFromFunctions("DateTimeHelpers",
        [
            kernel.CreateFunctionFromMethod(() => $"{DateTime.UtcNow:r}", "Now", "Gets the current date and time")
        ]);

        KernelFunction qa = kernel.CreateFunctionFromPrompt("""
    The current date and time is {{ datetimehelpers.now }}.
    {{ $input }}
    """);

        // Q&A loop
        var arguments = new KernelArguments();
        while (true)
        {
            Console.Write("Question: ");
            arguments["input"] = Console.ReadLine();
            Console.WriteLine(await qa.InvokeAsync(kernel, arguments));
            Console.WriteLine();
        }
    }
}
