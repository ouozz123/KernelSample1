using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using KernelSample.Qdrant.Model;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client.Grpc;
using System.Text.Json;
using QdrantClient = Qdrant.Client.QdrantClient;

namespace KernelSample.Qdrant; 

/// <summary>
/// 提供旅館資訊匯入至Qdrant
/// </summary>
internal class ImportHotalDataForQdrantDemo : Sample
{
    internal override async Task RunAsync(string apiKey)
    {
        var dimensions = 1536; // 向量的維度,原本是128
        var httpClient = HttpLogger.GetHttpClient(true);
        var embeddingModelName = "text-embedding-3-small";

#pragma warning disable SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var kernelBuilder = Kernel
            .CreateBuilder()
            .AddQdrantVectorStore("localhost")
            .AddOpenAITextEmbeddingGeneration(
                    modelId: embeddingModelName,
                    apiKey: apiKey,
                    httpClient: httpClient, 
                    dimensions: dimensions             
                )
#pragma warning restore SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

            .Build();

        //OpenAI轉向量服務服務
        var embeddingGenerationService = kernelBuilder.GetRequiredService<ITextEmbeddingGenerationService>();

        #region 取得旅館資訊，並轉換為 Qdrent Table Model

        // var text = await ReadFileLinesAsync("旅宿列表匯出_20250401113015_part2.txt");
        // var models = await ConvertModel(text, embeddingGenerationService, true);

        #endregion

        // 建立 LoggerFactory 實例
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole() // 添加控制台日誌提供者
                .SetMinimumLevel(LogLevel.Debug); // 設置最低日誌級別
        });

        var channel = GrpcChannel.ForAddress("http://localhost:6334", new GrpcChannelOptions
        {
            //HttpClient = httpClient,
            //LoggerFactory = loggerFactory,
        }).Intercept(new LoggingInterceptor(loggerFactory));

        var qdrantGrpcClient = new QdrantGrpcClient(channel);
        var qdrantClient = new QdrantClient(qdrantGrpcClient);   

        // var grpcClient = new QdrantGrpcClient("localhost");
        // var qdrant = new QdrantClient(grpcClient, loggerFactory: loggerFactory);
        var vectorStore = new QdrantVectorStore(qdrantClient, new QdrantVectorStoreOptions() { HasNamedVectors = false });
        var hotelCollectionName = "hotel2";

        var hotel2 = vectorStore.GetCollection<ulong, Hotel2>(hotelCollectionName);

        // ////建立 Qdrant旅館 Table
        // //await hotel2.DeleteCollectionAsync();
        // await hotel2.CreateCollectionIfNotExistsAsync();

        // //建立旅館資料
        // await foreach (var result in hotel2.UpsertBatchAsync(models))
        //     Console.WriteLine($"成功更新或插入的 ID：{result}");

        //向量搜尋
        var keyword2 = await embeddingGenerationService.GenerateEmbeddingAsync("請問南投縣的旅館有哪些?");
        var record2 = await hotel2.VectorizedSearchAsync(keyword2, new()
        {
            Top = 3 //只抓三筆
        });

        if (record2 != null)
           await foreach (var item in record2.Results)
                Console.WriteLine($"2.搜尋結果：{item.Record.HotelName} - {item.Record.Description} - {item.Score}");
    }

    private static async Task<List<string>> ReadFileLinesAsync(string fileName)
    {
        //取得專案
        string projectDirectory = Directory.GetCurrentDirectory();
        string filePath = Path.Combine(projectDirectory, "Files", fileName);
        Console.WriteLine("filePath:{0}", filePath);

        List<string> lines = new List<string>();
        if (File.Exists(filePath))
        {
            Console.WriteLine("檔案存在");
            await foreach (var line in ReadLinesAsync(filePath))
                lines.Add(line);
        }
        else
        {
            Console.WriteLine("檔案不存在");
        }

        return lines;
    }
    private static async IAsyncEnumerable<string> ReadLinesAsync(string filePath)
    {
        using var streamReader = new StreamReader(filePath);
        while (!streamReader.EndOfStream)
        {
            yield return await streamReader.ReadLineAsync();
        }
    }

#pragma warning disable SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
    private async Task<List<Hotel2>> ConvertModel(List<string> lines, ITextEmbeddingGenerationService services, bool isEmbedding = false)
#pragma warning restore SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
    {
        List<Hotel2> hotels = new List<Hotel2>();
        ulong i = 1;
        foreach (var item in lines)
        {
            var itemArr = item.Split(',');

            var hotel = new Hotel2()
            {
                HotelId = i++,
                // OpeningDate = itemArr[0],
                HotelName = itemArr[5],
                Address = itemArr[9]
            };

            hotel.Description = $"旅館名稱:{hotel.HotelName}，旅館縣市:{itemArr[6]}，旅館地址:{hotel.Address}。";
            Console.WriteLine("hotel.Description: {0}", hotel.Description);

            if (isEmbedding)
                hotel.DescriptionEmbedding = await services.GenerateEmbeddingAsync(hotel.Description);

            hotels.Add(hotel);
        }

        Console.WriteLine("hotels: {0}", JsonSerializer.Serialize(hotels));

        return hotels;
    }
}
