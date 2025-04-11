using KernelSample.Plugin.HotelPlugin;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MongoDB.Bson;

namespace KernelSample.Qdrant;

internal class HotelPlugnDemo : Sample
{
    internal const string QdrantServiceId = "QdrantServiceId";
    internal const string HotelCollectionName = "hotel2";


    internal override async Task RunAsync(string apiKey)
    {
        var dimensions = 1536; // 向量的維度,原本是128
        var httpClient = HttpLogger.GetHttpClient(true);
        var embeddingModelName = "text-embedding-3-small";

        // 建立 LoggerFactory 實例
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole() // 添加控制台日誌提供者
                .SetMinimumLevel(LogLevel.Debug); // 設置最低日誌級別
        });

#pragma warning disable SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var kernelBuilder = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-4o-mini",
                apiKey: apiKey,
                httpClient: httpClient
            )
            .AddCustomQdrantVectorStore("http://localhost:6334", loggerFactory, serviceId: QdrantServiceId)
            .AddOpenAITextEmbeddingGeneration(
                    modelId: embeddingModelName,
                    apiKey: apiKey,
                    httpClient: httpClient, 
                    dimensions: dimensions
                );
#pragma warning restore SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

        kernelBuilder.Plugins.AddFromType<HotelPlugin>();
        var kernel = kernelBuilder.Build();

        ////get OpenAIChatCompletion
        //var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        //var chatHistory = new ChatHistory();
        //chatHistory.AddUserMessage("請給我台北市旅館資訊");
        OpenAIPromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
        KernelArguments arguments = new(settings);
        var result = await kernel.InvokePromptAsync("請給我台北市 hotel 資訊", arguments);
        Console.WriteLine(result);
    }
}
