using KernelSample.Plugin.HotelPlugin;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Memory;
using MongoDB.Bson;
using System.Text.Json;

namespace KernelSample.Qdrant;

internal class HotelPlugnSample : Sample
{
    internal override async Task RunAsync(string apiKey)
    {
        var dimensions = 1536; // 向量的維度,原本是128
        var httpClient = HttpLogger.GetHttpClient(true);
        var qarantHttpClient = "http://localhost:6333";
        var embeddingModel = "text-embedding-3-small";

#pragma warning disable SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var embeddingGeneration = new OpenAITextEmbeddingGenerationService(
            modelId: embeddingModel,
            httpClient: httpClient,
            apiKey: apiKey);

        var memoryBuilder = new MemoryBuilder();
        var memory = memoryBuilder
            .WithQdrantMemoryStore(httpClient, dimensions, qarantHttpClient)
            .WithTextEmbeddingGeneration(embeddingGeneration)
            .Build();
#pragma warning restore SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

        //kernel 處理插件跟 Planning 相關
#pragma warning disable SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var builder = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion("gpt-4o-mini", apiKey);
#pragma warning restore SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

        //新增套件
        builder.Plugins.AddFromObject(new HotelPlugin(memory));
        var kernel = builder.Build();

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();


        //直接調用 Plugin 中的方法
        var pluginSearchResult = await kernel.Plugins.GetFunction("HotelPlugin", "SearchHotel").InvokeAsync(kernel, new()
        {
            {"keyword", "查詢台北旅館" }
        });


        //自動偵測Prompt是否需使用插件方法
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("查詢旅館資訊");
        var result = await chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings: openAIPromptExecutionSettings,
            kernel: kernel);


        //IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        //var chatHistory = new ChatHistory();
        //chatHistory.AddUserMessage("search hotel");
        //var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        //Console.WriteLine(JsonSerializer.Serialize(result));
    }
}
