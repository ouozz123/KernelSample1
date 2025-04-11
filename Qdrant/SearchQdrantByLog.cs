using Amazon.Runtime.Internal.Util;
using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using KernelSample.Qdrant.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using NetTopologySuite.Geometries;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Text;
using System.Text.Json;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace KernelSample.Qdrant;
internal class SearchQdrantByLog : Sample
{
    internal override async Task RunAsync(string apiKey)
    {
        var dimensions = 1536; // 向量的維度,原本是128
        var httpClient = HttpLogger.GetHttpClient(true);
        var qarantHttpClient = "http://localhost:6333";
        var embeddingModel = "text-embedding-3-small";

        #region 建立 logger
        Console.OutputEncoding = Encoding.UTF8;

        // 建立 LoggerFactory 實例
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole((x) =>
                {
                }) 
                .SetMinimumLevel(LogLevel.Debug); // 設置最低日誌級別
        });

        #endregion

#pragma warning disable SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var embeddingGeneration = new OpenAITextEmbeddingGenerationService(
            modelId: embeddingModel,
            httpClient: httpClient,
            apiKey: apiKey);


        var builder = Kernel.CreateBuilder();

        var loggingInterceptor = new LoggingInterceptor(loggerFactory);

        var channel = GrpcChannel.ForAddress("http://localhost:6334", new GrpcChannelOptions
        {
            HttpClient = httpClient,
            //LoggerFactory = loggerFactory,

        });

        var interceptingChannel = channel.Intercept(loggingInterceptor);
        var qdrantGrpcClient = new QdrantGrpcClient(interceptingChannel);
        var qdrantClient = new QdrantClient(qdrantGrpcClient);   

        var kernel = Kernel.CreateBuilder()
            .AddCustomQdrantVectorStore(qdrantClient)
            .AddOpenAITextEmbeddingGeneration(
                    modelId: embeddingModel, // Name of the embedding model, e.g. "text-embedding-ada-002".
                    apiKey: apiKey,
                    //orgId: "YOUR_ORG_ID",         // Optional organization id.
                    //serviceId: "YOUR_SERVICE_ID", // Optional; for targeting specific services within Semantic Kernel
                    httpClient: HttpLogger.GetHttpClient(true), // Optional; if not provided, the HttpClient from the kernel will be used 
                    dimensions: 1536              // Optional number of dimensions to generate embeddings with.
                )
            .Build();
#pragma warning restore SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

        var hotelCollectionName = "hotel2";
        var qdrantVectorStoreOptions = new QdrantVectorStoreOptions() { HasNamedVectors = false };
        var vectorStore = new QdrantVectorStore(qdrantClient, qdrantVectorStoreOptions);
        var hotel = vectorStore.GetCollection<ulong, Hotel2>(hotelCollectionName);

        var embeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        var keyword = await embeddingGenerationService.GenerateEmbeddingAsync("詢問台北旅館有哪些");

        var searchResult = await hotel.VectorizedSearchAsync(keyword, new VectorSearchOptions<Hotel2>()
        {
            Top = 3,
            //VectorProperty = r => r.DescriptionEmbedding
        });

        await foreach (var item in searchResult.Results)
            Console.WriteLine($"查詢結果：{JsonSerializer.Serialize(item)}");
    }
}
