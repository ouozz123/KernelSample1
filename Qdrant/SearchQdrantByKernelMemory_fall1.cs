using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Memory;
using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace KernelSample.Qdrant;
internal class SearchQdrantByKernelMemory_fall1 : Sample
{
    internal override async Task RunAsync(string apiKey)
    {
        var dimensions = 1536; // 向量的維度,原本是128
        var httpClient = HttpLogger.GetHttpClient(true);
        var qarantHttpClient = "http://localhost:6333";
        var embeddingModel = "text-embedding-3-small";

        #region 建立 logger

        // 建立 LoggerFactory 實例
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole() // 添加控制台日誌提供者
                .SetMinimumLevel(LogLevel.Debug); // 設置最低日誌級別
        });

        #endregion

#pragma warning disable SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var embeddingGeneration = new OpenAITextEmbeddingGenerationService(
            modelId: embeddingModel,
            httpClient: httpClient,
            apiKey: apiKey);

        var memory = new KernelMemoryBuilder()
            .WithOpenAITextGeneration(new () { EmbeddingModel= embeddingModel, TextModel = "gpt-4o-mini", APIKey = apiKey })
            .WithQdrantMemoryDb("http://localhost:6333", null)
            .WithSemanticKernelTextEmbeddingGenerationService(embeddingGeneration, new()
            {
                MaxTokenTotal = 4096,
            }, loggerFactory: loggerFactory)
            .Build<MemoryServerless>(new KernelMemoryBuilderBuildOptions()
            {
                AllowMixingVolatileAndPersistentData = true
            });
        //.WithQdrantMemoryStore(httpClient, dimensions, qarantHttpClient)
        //.WithTextEmbeddingGeneration
        //.Build();
#pragma warning restore SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

        var result = await memory.AskAsync("請幫我查詢台北市有哪些旅館");
        Console.WriteLine(result.ToJson());

        //var qdrantVectorStoreOptions = new QdrantVectorStoreOptions() { HasNamedVectors = false };

        //查詢旅館有哪些
        //var collections = await memory.GetCollectionsAsync(); //This call triggers a subsequent call to Qdrant memory store.
        //Console.WriteLine($"Collections: {string.Join(", ", collections)}");

        //搜尋
        //var hotelCollectionName = "hotel2";
        //var searchResult = memory.SearchAsync(hotelCollectionName, "查詢南投縣有哪些旅館", minRelevanceScore: 0.3).ConfigureAwait(false);

        //await foreach (var item in searchResult)
        //{
        //    //轉json輸出
        //    var json = item.ToJson();
        //    Console.WriteLine(json);
        //}
    }
}
