using KernelSample.Plugin.HotelPlugin;
using KernelSample.Qdrant.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client.Grpc;
using System.Text.Json;
using Document = Microsoft.KernelMemory.Document;
using QdrantClient = Qdrant.Client.QdrantClient;

namespace KernelSample.Qdrant; // static import Conditions to easily build filtering

internal class ImportHotalDataForQdrantSampleByHotel2 : Sample
{
    private static MemoryWebClient memoryServiceClient = null!;


    internal override async Task RunAsync(string apiKey)
    {
        var dimensions = 1536; // 向量的維度,原本是128
        var httpClient = HttpLogger.GetHttpClient(true);

#pragma warning disable SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var kernelBuilder = Kernel
            .CreateBuilder()
            .AddQdrantVectorStore("localhost")
            .AddOpenAITextEmbeddingGeneration(
                    modelId: "text-embedding-3-small", // Name of the embedding model, e.g. "text-embedding-ada-002".
                    apiKey: apiKey,
                    //orgId: "YOUR_ORG_ID",         // Optional organization id.
                    //serviceId: "YOUR_SERVICE_ID", // Optional; for targeting specific services within Semantic Kernel
                    httpClient: httpClient, // Optional; if not provided, the HttpClient from the kernel will be used 
                    dimensions: dimensions              // Optional number of dimensions to generate embeddings with.
                )
#pragma warning restore SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

            .Build();

        kernelBuilder.Plugins.AddFromType<HotelPlugin>();

        var hotelCollectionName = "hotel2";
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

        var grpcClient = new QdrantGrpcClient("localhost");
        var qdrant = new QdrantClient("localhost1", loggerFactory: loggerFactory);
        var vectorStore = new QdrantVectorStore(qdrant, qdrantVectorStoreOptions);

        var hotel2 = vectorStore.GetCollection<ulong, Hotel2>(hotelCollectionName);
        var embeddingGenerationService = kernelBuilder.GetRequiredService<ITextEmbeddingGenerationService>();

        #region 取得檔案資訊

        //var text = await ReadFileLinesAsync("旅宿列表匯出_20250401113015_part2.txt");
        //var models = await ConvertModel(text, embeddingGenerationService, true);

        #endregion

        #region 其他查詢方式
        // var keyword = await embeddingGenerationService.GenerateEmbeddingAsync("請問位於臺中市大雅區西寶里雅潭路的旅館有哪些?");

        // var result = await qdrant.SearchAsync(hotelCollectionName, keyword);

        // foreach (var item in result)
        // {
        //     Console.WriteLine($"找到匹配：{item.Payload} - {item.Score} ");
        // }  
        #endregion

        #region 建 table 跟新增資料

        //await hotel2.DeleteCollectionAsync();
        //await hotel2.CreateCollectionIfNotExistsAsync();

        //await foreach (var upsertResult in hotel2.UpsertBatchAsync(models))
        //    Console.WriteLine($"成功更新或插入的 ID：{upsertResult}");

        #endregion

        #region hotel GetAsync

        // //案例1: 用KEY搜尋
        // var record1 = await hotel2.GetAsync(1, new() { IncludeVectors = false });

        // if (record1 != null)
        //     // Console.WriteLine($"1.搜尋結果：{record1.Data["HotelName"]} - {record1.Data["Description"]}");
        //     Console.WriteLine($"搜尋結果：{record1.HotelName} - {record1.Description}"); weq 

        // Console.WriteLine("--------------------------------------------------");

        #endregion

        #region 向量查詢

        //案例2: 用向量搜尋
        var keyword2 = await embeddingGenerationService.GenerateEmbeddingAsync("請問南投縣的旅館有哪些?");
        var record2 = await hotel2.VectorizedSearchAsync(keyword2, new()
        {
            Top = 3
        });

        if (record2 != null)
           await foreach (var item in record2.Results)
                Console.WriteLine($"2.搜尋結果：{item.Record.HotelName} - {item.Record.Description} - {item.Score}");

        #endregion
    }

    private async Task CreateQdrantProint(List<Hotel> models, IVectorStoreRecordCollection<ulong, Hotel> collection)
    {
        //foreach (var item in models)
        //{
        //   var result = await hotel.UpsertAsync(item);
        //    Console.WriteLine($"成功更新或插入的 ID：{result}");
        //}

        await foreach (var result in collection.UpsertBatchAsync(models))
        {
            Console.WriteLine($"成功更新或插入的 ID：{result}");
        }
    }

    /// <summary>
    /// Creates a new collection after deleting an existing one, configuring it with specific vector parameters.
    /// </summary>
    /// <param name="qdrant">Used to interact with the Qdrant database for collection management.</param>
    /// <param name="hotelCollectionName">Specifies the name of the collection to be deleted and recreated.</param>
    /// <returns>No return value as the method performs asynchronous operations on the collection.</returns>
    private async Task CreateOrDeleteCollection(IVectorStoreRecordCollection<ulong, Hotel> collection, string hotelCollectionName)
    {
        //刪除 collection
        await collection.DeleteCollectionAsync();

        //////重新建立 collection 
        //var vectorsConfig = new VectorParams
        //{
        //    Size = 1536,  // 向量的維度,原本是128
        //    Distance = Distance.Cosine  // 設定測量方式為 Cosine 相似度\
        //    //MultivectorConfig = new MultiVectorConfig()
        //    //{
        //    //    Comparator = MultiVectorComparator.MaxSim
        //    //}
        //};
        await collection.CreateCollectionAsync();
        //await collection.RecreateCollectionAsync(vectorsConfig);
    }

    //static async Task ImportXlsxToQdrant(IKernelMemory memory, string filePath)
    //{
    //    var texts = ReadXlsxFile(filePath);
    //    foreach (var text in texts)
    //    {
    //        await memory.ImportTextAsync(
    //            text,
    //            documentId: Guid.NewGuid().ToString(),
    //            tags: new TagCollection { { "source", "xlsx" } }
    //        );
    //    }
    //    Console.WriteLine("資料已成功匯入 Qdrant！");
    //}

    //static List<string> ReadXlsxFile(string filePath)
    //{
    //    var texts = new List<string>();
    //    using (var workbook = new XLWorkbook(filePath))
    //    {
    //        var worksheet = workbook.Worksheet(1);
    //        var rows = worksheet.RowsUsed();
    //        foreach (var row in rows)
    //        {
    //            string description = row.Cell(2).GetString();
    //            if (!string.IsNullOrEmpty(description))
    //            {
    //                texts.Add(description);
    //            }
    //        }
    //    }
    //    return texts;
    //}

    static async Task SearchInQdrant(IKernelMemory memory, string query)
    {
        var results = await memory.SearchAsync(query, limit: 5);
        foreach (var result in results.Results)
        {
            //Console.WriteLine($"找到匹配：{result.Relevance} - {result.Text}");
        }
    }

    private static async Task StoreExcel()
    {
        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        string filePath = Path.Combine(projectDirectory, "Files", "旅宿列表匯出_20250401113015_part1.txt");
        Console.WriteLine("filePath:{0}", filePath);

        if (File.Exists(filePath))
            Console.WriteLine("檔案已存在");

        if (!await memoryServiceClient.IsDocumentReadyAsync(documentId: "hotel-list-01"))
        {
            Console.WriteLine($"上傳 「{filePath}」 至 Memory Service");
            var docId = await memoryServiceClient.ImportDocumentAsync(new Document("hotel-list-01").AddFiles([filePath]));
            Console.WriteLine($"- Document Id: {docId}");
        }
        else
        {
            Console.WriteLine($"「{filePath}」 already uploaded.");
        }
    }

    private static async Task<List<string>> ReadFileLinesAsync(string fileName)
    {
        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
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

    private void SaveVectorFile(List<Hotel> hotels)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(hotels, options);
        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        var filePath = Path.Combine(projectDirectory, "Files", "Vector", "旅宿列表匯出_20250401113015_part1.json");
        File.WriteAllText(filePath, json);
    }
}
