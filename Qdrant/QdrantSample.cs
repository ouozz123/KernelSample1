using KernelSample.Qdrant.Model;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using System.Text.Json;

namespace KernelSample.Qdrant;
internal class QdrantSample : Sample
{
    internal override async Task RunAsync(string apiKey)
    {
#pragma warning disable SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var kernelBuilder = Kernel
            .CreateBuilder()
            .AddQdrantVectorStore("localhost")
            .AddOpenAITextEmbeddingGeneration(
                    modelId: "text-embedding-3-small", // Name of the embedding model, e.g. "text-embedding-ada-002".
                    apiKey: apiKey,
                    //orgId: "YOUR_ORG_ID",         // Optional organization id.
                    //serviceId: "YOUR_SERVICE_ID", // Optional; for targeting specific services within Semantic Kernel
                    httpClient: new HttpClient(), // Optional; if not provided, the HttpClient from the kernel will be used
                    dimensions: 1536              // Optional number of dimensions to generate embeddings with.
                )
#pragma warning restore SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

            .Build();

#pragma warning disable SKEXP0020 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        //var client = new QdrantVectorDbClient("http://localhost:6333", 1536);
#pragma warning restore SKEXP0020 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

        var collectionName = "skhotels";
        var collection = new QdrantVectorStoreRecordCollection<Hotel>(new QdrantClient("localhost"), "skhotels");

#pragma warning disable S1481 // Unused local variables should be removed
        var embeddingService = kernelBuilder.GetRequiredService<ITextEmbeddingGenerationService>();
#pragma warning restore S1481 // Unused local variables should be removed


        //var hotels = new List<Hotel>() {
        //    new Hotel()
        //    {
        //        HotelId = 3,
        //        HotelName = "天堂鳥",
        //    },
        //    new Hotel()
        //    {
        //        HotelId = 1,
        //        HotelName = "札幌的民宿",
        //    },
        //    new Hotel()
        //    {
        //        HotelId = 2,
        //        HotelName = "小樽的民宿",
        //    }
        //};
        //await Task.WhenAll(hotels.Select(h => Task.Run(async () => h.DescriptionEmbedding = await embeddingService.GenerateEmbeddingAsync(h.Description))));


        //var hotal = new Hotel()
        //{
        //    HotelId = 2,
        //    HotelName = "小樽的民宿",
        //    Description = "這家位於小樽的民宿以其美麗的海景和現代化設施著稱。小樽是一個充滿歷史的城市，擁有浪漫的運河和眾多的小吃店。這家民宿提供免費的自製早餐、舒適的床鋪，以及非常友善的主人，讓住客感受到家的溫暖。"
        //};
        //hotal.DescriptionEmbedding =  await embeddingService.GenerateEmbeddingAsync(hotal.Description);
        //var result = await collection.UpsertAsync(hotal);
        //var result = collection.UpsertBatchAsync(hotels).GetAsyncEnumerator().ConfigureAwait(true);

        //向量查詢
        var keyword = await embeddingService.GenerateEmbeddingAsync("小樽的民宿");
        var opions = new VectorSearchOptions<Hotel>()
        {
            Top = 1
        };

        var searchResult =  await collection.VectorizedSearchAsync(keyword, opions);
        var results = searchResult.Results;

        Console.WriteLine("searchResult: {0}", JsonSerializer.Serialize(results));

        //memoryServiceClient = new MemoryWebClient("http://127.0.0.1:9001/");
        //var textSearch = new VectorStoreTextSearch<Hotel>(vectorStoreRecordCollection, textEmbeddingGeneration);
        //var point = new PointStruct
        //{
        //    Id = (ulong)2,
        //    Vectors = Enumerable.Range(1, 100).Select(_ => (float)random.NextDouble()).ToArray(),
        //    Payload =
        //      {
        //        ["color"] = "red",
        //        ["rand_number"] = 4
        //      }
        //};

        //var vectorsConfig = new VectorParams
        //{
        //    Size = 1536,  // 向量的維度 原128
        //    Distance = Distance.Cosine,  // 設定測量方式為 Cosine 相似度
        //    //MultivectorConfig = new MultiVectorConfig()
        //    //{
        //    //    Comparator = MultiVectorComparator.MaxSim
        //    //}
        //};

        ////// 創建 Collection，並設置向量配置
        //try
        //{
        //    await client.RecreateCollectionAsync(collectionName, vectorsConfig);
        //    Console.WriteLine($"Collection '{collectionName}' created with vectors of size {vectorsConfig.Size} and distance {vectorsConfig.Distance}.");
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"Error creating collection: {ex.Message}");
        //}

        //var hotelCollection =  vectorStore.GetCollection<ulong, Hotel>(collectionName);

        //新增索引
        //var result = client.CreatePayloadIndexAsync(collectionName, "");

        //var collectionInfo = await client.GetCollectionInfoAsync(collectionName);
        //Console.WriteLine("collectionInfo: {0}", JsonSerializer.Serialize(hotel));

        //Console.WriteLine(collectionInfo);

        //await client.UpsertVectorsAsync(
        //    collectionName,
        //    [ConvertToQdrantVectorRecord(hotel, await embeddingService.GenerateEmbeddingAsync(hotel.Description))]
        //);

        //await IngestDataIntoVectorStoreAsync(hotelCollection, hotel, embeddingService);
    }


    /// <summary>
    /// Ingest data into the given collection.
    /// </summary>
    /// <param name="collection">The collection to ingest data into.</param>
    /// <param name="textEmbeddingGenerationService">The service to use for generating embeddings.</param>
    /// <returns>The keys of the upserted records.</returns>
    internal static async Task<ulong> IngestDataIntoVectorStoreAsync(
        IVectorStoreRecordCollection<ulong, Hotel> collection,
        Hotel hotel,
#pragma warning disable SKEXP0001 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        ITextEmbeddingGenerationService textEmbeddingGenerationService)
#pragma warning restore SKEXP0001 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
    {
        // Create the collection if it doesn't exist.
        //await collection.CreateCollectionIfNotExistsAsync();

        // Create glossary entries and generate embeddings for them.
        hotel.DescriptionEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(hotel.Description);

        Console.WriteLine("hotel: {0}", JsonSerializer.Serialize(hotel));


        // Upsert the glossary entries into the collection and return their keys.
        return await collection.UpsertAsync(hotel);
    }

//#pragma warning disable SKEXP0020 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
//    public static PointStruct ConvertToQdrantVectorRecord(Hotel hotel, ReadOnlyMemory<float> embedding)
//#pragma warning restore SKEXP0020 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
//    {
//        // 將酒店的資料轉換為 Payload
//        var payload = new Dictionary<string, object>
//        {
//            { "hotel_name", hotel.HotelName },
//            { "description", hotel.Description }
//        };

//        // 創建 QdrantVectorRecord，使用 HotelId 作為 PointId
//        var pointId = hotel.HotelId.ToString();

//        // 建立 QdrantVectorRecord 實例
//#pragma warning disable SKEXP0020 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
//        var vectorRecord = new QdrantVectorRecord(
//            pointId: pointId,
//            embedding: embedding,
//            payload: payload
//        );
//#pragma warning restore SKEXP0020 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

//        return vectorRecord;
//    }
}


