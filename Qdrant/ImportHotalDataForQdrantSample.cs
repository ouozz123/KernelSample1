using KernelSample.Qdrant.Model;
using Microsoft.Extensions.VectorData;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using System.Text.Json;
using Document = Microsoft.KernelMemory.Document;
using QdrantClient = Qdrant.Client.QdrantClient;

namespace KernelSample.Qdrant;
internal class ImportHotalDataForQdrantSample : Sample
{
    private static MemoryWebClient memoryServiceClient = null!;


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
                    httpClient: HttpLogger.GetHttpClient(true), // Optional; if not provided, the HttpClient from the kernel will be used 
                    dimensions: 1536              // Optional number of dimensions to generate embeddings with.
                )
#pragma warning restore SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

            .AddQdrantVectorStore()
            .Build();


        var embeddingGenerationService = kernelBuilder.GetRequiredService<ITextEmbeddingGenerationService>();

        ////從檔案中取得資料，並轉為 Hotel model，並儲存在 json 檔案中
    
        var fileName = "旅宿列表匯出_20250401113015_part1.txt";
        var vectorFileName = "旅宿列表匯出_20250401113015_part1.json";
        var vectorFilePath = PathHelper.GetFullFilePath(Path.Combine("Files", "Vector", vectorFileName));
        var models = new List<Hotel>();
        if (File.Exists(vectorFilePath))
        {
            var json = File.ReadAllText(vectorFilePath)!;
            models = JsonSerializer.Deserialize<List<Hotel>>(json);
        }
        else
        {
            var lines = await ReadFileLinesAsync(fileName);
            models = await ConvertModel(lines, embeddingGenerationService, true);

            if(models != null && models.Any())
                SaveVectorFile(models);
        }

        //初始化
        var qdrant = new QdrantClient("localhost");

        var hotelCollectionName = "skhotels";

        var qdrantVectorStoreOptions = new QdrantVectorStoreOptions() { HasNamedVectors = false };
        var vectorStore = new QdrantVectorStore(qdrant, qdrantVectorStoreOptions);
        var hotel = vectorStore.GetCollection<ulong, Hotel>(hotelCollectionName);
        //if (!await hotel.CollectionExistsAsync())
        //await CreateOrDeleteCollection(hotel, hotelCollectionName);

        //////更新資料至 Qdrant
        //if (models != null && models.Any())
        //    await CreateQdrantProint(models, hotel);


        //查詢 Qdrant
        var question = "台灣南投縣有哪些民宿?";
        var keyword = await embeddingGenerationService.GenerateEmbeddingAsync(question);
        var options = new VectorSearchOptions<Hotel>
        {
            Top = 1,
            VectorProperty = r => r.DescriptionEmbedding
        };

        //Failed to convert vector store record.
        var searchResult = await hotel.VectorizedSearchAsync(keyword, options);

        Console.WriteLine($"查詢結果：");
        await foreach (var item in searchResult.Results)
            Console.WriteLine($"查詢結果：{JsonSerializer.Serialize(item)}");

        ////將資料儲存至 Qdrant
        //foreach (var model in models)
        //{
        //    await memory.ImportTextAsync(
        //        model.Description,
        //        documentId: model.HotelId.ToString(),
        //        tags: new TagCollection { { "source", "hotel" } }
        //    );
        //}

        //var collectionName = "skhotels";
        //var vectorStore = new QdrantVectorStore(new QdrantClient("localhost"));
        //var recordCollection = vectorStore.GetCollection<ulong, Hotel>(collectionName);
        //await recordCollection.CreateCollectionIfNotExistsAsync().ConfigureAwait(false);

        //var stringMapper = new HotelTextSearchStringMapper();
        //var resultMapper = new HotelTextSearchResultMapper();

        //// 使用內存矢量存儲創建文本搜索實例
        //var textSearch = new VectorStoreTextSearch<Hotel>(recordCollection, textEmbeddingGeneration, stringMapper, resultMapper);

        //// 搜索和返回結果作為文本搜索結果項目
        //var query = "給我天堂鳥相關的資訊";
        ////向量查詢
        //var keyword = await embeddingService.GenerateEmbeddingAsync(query);
        //var opions = new VectorSearchOptions<Hotel>()
        //{
        //    Top = 1
        //};

        //var searchResult = await collection.VectorizedSearchAsync(keyword, opions);
        //var results = searchResult.Results;

        //Console.WriteLine("searchResult: {0}", JsonSerializer.Serialize(results));


        //從 Qdrant 進行查詢
        //var query = "給我天堂鳥相關的資訊";
        //var results = await memory.SearchAsync(query, limit: 5);
        //foreach (var result in results.Results)
        //{
        //    Console.WriteLine($"找到匹配：{result.Relevance} - {result.Text}");
        //}



        //從 memory service 進行查詢
        //var question = "幫我從 hotel-list-01 檔案中，幫我擷取標章顯示為「好客民宿」的旅館資訊，截取的資訊要有資訊類別(class)、標章(tag)、旅宿名稱(title)、縣市(city)、鄉鎮(area)、地址(address)、電話或手機(phone)、網址(link)，必須要是檔案內提供的，若沒有符合的請回應空陣列即可，請用 json 格式提供給我，符合的資料都提供，不包含其他說明";
        //Console.WriteLine($"Question: {question}");
        //Console.WriteLine("Expected result: [{ class:資訊類別, tag:標章, title:旅宿名稱, city:縣市, area:鄉鎮, address:地址, phone:電話或手機, link:網址  }, ...]");


        ////AskAsync
        //var searchResult = await memoryServiceClient.AskAsync(question, null, filter: MemoryFilters.ByDocument("hotel-list-01"));

        //Console.Write($"answer: {JsonSerializer.Serialize(searchResult.Result)}");
        //Console.Write($"RelevantSources: {JsonSerializer.Serialize(searchResult.RelevantSources)}");

        //SearchAsync
        //var searchResult = await memoryServiceClient.SearchAsync(
        //   question, null, filter: MemoryFilters.ByDocument("hotel-list-01"), limit: 3);

        //var result = searchResult.Results;

        //foreach (var answer in result)
        //{
        //    Console.Write($"answer: {JsonSerializer.Serialize(answer)}");
        //}


        //var answerStream = memoryServiceClient.AskStreamingAsync(
        //        question, null, filter: MemoryFilters.ByDocument("hotel-list-01"), options: new SearchOptions { Stream = true });

        //List<Citation> sources = [];
        //await foreach (var answer in answerStream)
        //{
        //    // Print token received by LLM
        //    Console.Write(answer.Result);

        //    // Collect sources
        //    sources.AddRange(answer.RelevantSources);

        //    // Slow down the stream for demo purpose
        //    await Task.Delay(5);
        //}

        //// Show sources / citations
        //Console.WriteLine("\n\nSources:\n");
        //foreach (var x in sources)
        //{
        //    Console.WriteLine(x.SourceUrl != null
        //        ? $"  - {x.SourceUrl} [{x.Partitions.First().LastUpdate:D}]"
        //        : $"  - {x.SourceName}  - {x.Link} [{x.Partitions.First().LastUpdate:D}]");
        //}

        //Console.WriteLine("\n====================================\n");

        // 匯入 XLSX
        //await ImportXlsxToQdrant(memory, filePath);

        //// 查詢測試
        //await SearchInQdrant(memory, "關鍵詞或問題");

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
        string filePath = Path.Combine(projectDirectory, "Files", "旅宿列表匯出_20250401113015_part1.txt");
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
    private async Task<List<Hotel>> ConvertModel(List<string> lines, ITextEmbeddingGenerationService services, bool isEmbedding = false)
#pragma warning restore SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
    {
        List<Hotel> hotels = new List<Hotel>();
        ulong i = 1;
        foreach (var item in lines)
        {
            var itemArr = item.Split(',');

            var hotel = new Hotel()
            {
                HotelId = i++,
                OpeningDate = itemArr[0],
                Type = itemArr[2],
                Label = itemArr[3],
                IsSpringLabel = itemArr[4] == "是",
                HotelName = itemArr[5],
                CityName = itemArr[6],
                Area = itemArr[7],
                PostalCode = itemArr[8],
                Address = itemArr[9],
                Phone = itemArr[10],
                Email = itemArr[13],
                Link = itemArr[14]
            };

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
