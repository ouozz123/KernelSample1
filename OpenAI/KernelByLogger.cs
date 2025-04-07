using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace KernelSample.OpenAI;
internal class KernelByLogger : Sample
{
    internal override async Task RunAsync(string apiKey)
    {
        //dotnet add package Microsoft.SemanticKernel
        //dotnet add package Microsoft.Extensions.Logging
        //dotnet add package Microsoft.Extensions.Logging.Console


        // Populate values from your OpenAI deployment
        var modelId = "";
        var endpoint = "";

        // Create a kernel with Azure OpenAI chat completion
        var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);

        builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
        //builder.Plugins.AddFromType<TimePlugin>();
        Kernel kernel = builder.Build();
    }
}
