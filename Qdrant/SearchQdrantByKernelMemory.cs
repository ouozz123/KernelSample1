using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Memory;
using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace KernelSample.Qdrant;
internal class SearchQdrantByKernelMemory : Sample
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

        var builder = new MemoryBuilder();
        var memory = builder
            .WithQdrantMemoryStore(httpClient, dimensions, qarantHttpClient)
            .WithTextEmbeddingGeneration(embeddingGeneration)
            .Build();
#pragma warning restore SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

        var qdrantVectorStoreOptions = new QdrantVectorStoreOptions() { HasNamedVectors = false };

        #region 建立 logger

        // 建立 LoggerFactory 實例
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole() // 添加控制台日誌提供者
                .SetMinimumLevel(LogLevel.Debug); // 設置最低日誌級別
        });

        #endregion

        //查詢旅館有哪些
        //var collections = await memory.GetCollectionsAsync(); //This call triggers a subsequent call to Qdrant memory store.
        //Console.WriteLine($"Collections: {string.Join(", ", collections)}");

        var hotelCollectionName = "hotel2";
        var searchResult = memory.SearchAsync(hotelCollectionName, "查詢南投縣有哪些旅館", minRelevanceScore: 0.3);

        await foreach (var item in searchResult)
        {
            //轉json輸出
            var json = item.ToJson();
            Console.WriteLine(json);
        }

        //var search = await memory.SearchAsync(
        //    collection: hotelCollectionName,
        //    query: "給我天堂鳥相關的資訊",
        //    limit: 2,
        //    minRelevanceScore: 0.7,
        //    withEmbeddings: false); 

        //#pragma warning disable SKEXP0020 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        //        IQdrantVectorDbClient dbClinet = new QdrantVectorDbClient(
        //            httpClient,
        //            dimensions,
        //            "http://localhost:6333",
        //            loggerFactory);
        //#pragma warning restore SKEXP0020 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
    }
}
