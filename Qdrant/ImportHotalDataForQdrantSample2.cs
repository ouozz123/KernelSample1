using KernelSample.Qdrant.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using System.Text;
using System.Text.Json;
using Document = Microsoft.KernelMemory.Document;
using QdrantClient = Qdrant.Client.QdrantClient;

namespace KernelSample.Qdrant;
internal class ImportHotalDataForQdrantSample2 : Sample
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

            .Build();

        var hotelCollectionName = "skhotels";
        var qdrantVectorStoreOptions = new QdrantVectorStoreOptions() { HasNamedVectors = false };
        var qdrant = new QdrantClient("localhost");
        var vectorStore = new QdrantVectorStore(qdrant, qdrantVectorStoreOptions);
        var hotelDefinition = new VectorStoreRecordDefinition
        {
            Properties = new List<VectorStoreRecordProperty>
                {
                    new VectorStoreRecordKeyProperty("HotelId", typeof(ulong)),
                    new VectorStoreRecordDataProperty("Type", typeof(string)) { IsFilterable = true },
                    new VectorStoreRecordDataProperty("Label", typeof(string)) { IsFilterable = true },
                    new VectorStoreRecordDataProperty("IsSpringLabel", typeof(bool)) { IsFilterable = true },
                    new VectorStoreRecordDataProperty("CityName", typeof(string)) { IsFilterable = true },
                    new VectorStoreRecordDataProperty("Area", typeof(string)) { IsFilterable = true },
                    new VectorStoreRecordDataProperty("PostalCode", typeof(string)) { IsFilterable = false },
                    new VectorStoreRecordDataProperty("Phone", typeof(string)) { IsFilterable = false },
                    new VectorStoreRecordDataProperty("Email", typeof(string)) { IsFilterable = false },
                    new VectorStoreRecordDataProperty("Link", typeof(string)) { IsFilterable = false },
                    new VectorStoreRecordDataProperty("OpeningDate", typeof(string)) { IsFilterable = false },
                    new VectorStoreRecordDataProperty("Address", typeof(string)) { IsFilterable = false },
                    new VectorStoreRecordDataProperty("HotelName", typeof(string)) { IsFilterable = true },
                    new VectorStoreRecordDataProperty("Description", typeof(string)) { IsFullTextSearchable = true },
                    new VectorStoreRecordVectorProperty("DescriptionEmbedding", typeof(ReadOnlyMemory<float>)) { Dimensions = 1536, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw },
                }
        };

        // var hotel = vectorStore.GetCollection<ulong, VectorStoreGenericDataModel<ulong>>(
        //     hotelCollectionName,
        //     hotelDefinition);
        var hotel = vectorStore.GetCollection<ulong, Hotel>(hotelCollectionName);
        var embeddingGenerationService = kernelBuilder.GetRequiredService<ITextEmbeddingGenerationService>();

        // await hotel.DeleteCollectionAsync();
        // await hotel.CreateCollectionIfNotExistsAsync();

        //案例1: 用 KEY 搜尋
        var record1 = await hotel.GetAsync(1, new() { IncludeVectors = true });

        if (record1 != null)
            // Console.WriteLine($"1.搜尋結果：{record1.Data["HotelName"]} - {record1.Data["Description"]}");
            Console.WriteLine($"搜尋結果：{record1.HotelName} - {record1.Description}");

        // Console.WriteLine("--------------------------------------------------");

        // //案例2: 用向量搜尋
        // var keyword = await embeddingGenerationService.GenerateEmbeddingAsync("請問台北市的旅館有哪些?");
        // var record2 = await hotel.VectorizedSearchAsync(keyword, new(){
        //     Top = 5,
        //     VectorProperty = r => r.Data["DescriptionEmbedding"],
        // });

        // if (record2 != null)
        // {
        //     await foreach (var item in record2.Results)
        //         Console.WriteLine($"2.搜尋結果：{item.Record.Data["HotelName"]} - {item.Record.Data["Description"]}");
        // }
    }

    //static async Task (IKernelMemorImportXlsxToQdranty memory, string filePath)
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
                // OpeningDate = itemArr[0],
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
