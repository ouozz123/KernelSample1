using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using System.Text;

namespace KernelSample.KernelMemory;
internal class KernelMemorySearchDoc : Sample
{
    private const string QdrantServiceId = "QdrantServiceId";

    internal override async Task RunAsync(string apiKey)
    {
        var textModel = "gpt-4o-mini";
        var embeddingModel = "text-embedding-3-small";
        var openAIConfig = new OpenAIConfig()
        {
            APIKey = apiKey,
            TextModel = textModel,
            EmbeddingModel = embeddingModel,
            TextModelMaxTokenTotal = 16384,
            EmbeddingModelMaxTokenTotal = 8191,
        };
        var httpClient = HttpLogger.GetHttpClient(true);

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

        var memory = new KernelMemoryBuilder()
            .Configure(builder => 
                builder.Services.AddLogging(l =>
                {
                    l.SetMinimumLevel(LogLevel.Error);
                    l.AddSimpleConsole(c => c.SingleLine = true);
                }))
            //.WithOpenAIDefaults(Environment.GetEnvironmentVariable("OPENAI_API_KEY")!)
            //.WithOpenAIDefaults(apiKey, httpClient: httpClient)
            .WithOpenAITextGeneration(openAIConfig, httpClient: httpClient)
            .WithOpenAITextEmbeddingGeneration(openAIConfig, httpClient: httpClient)
                   //.WithQdrantMemoryDb("http://localhost:6333", QdrantServiceId)
                   //.WithCustomTextPartitioningOptions(new TextPartitioningOptions
                   //{
                   //    //定義每個分區的最大令牌數量
                   //    MaxTokensPerParagraph = 1000,
                   //    //定義每個分區與上一個分區的重疊令牌數量
                   //    //在您的代碼中，設置為 47，表示每個分區會包含上一個分區的最後 47 個令牌，確保上下文連續性。
                   //    OverlappingTokens = 50,
                   //})
            .WithSearchClientConfig(new SearchClientConfig
            {
                AnswerTokens = 10000,
                MaxMatchesCount = 50,
                MaxAskPromptSize = 16384
            })
            .WithSimpleFileStorage(new SimpleFileStorageConfig { StorageType = FileSystemTypes.Disk })
            .WithSimpleVectorDb(new SimpleVectorDbConfig { StorageType = FileSystemTypes.Disk })
            //.WithContentDecoder<CustomPdfDecoder>() // Register a custom PDF decoder
            .Build<MemoryServerless>();


        #region 純文字匯入並查詢
        //var textId = await memory.ImportTextAsync("豬八戒的真實名稱是朱重八");

        //var question = "豬八戒的真實名稱是？";
        //var answer = await memory.AskAsync(question);
        //Console.WriteLine($"\nAnswer: {answer.Result}");
        #endregion


        #region PDF檔案

        string miDocPath = Path.Combine(Environment.CurrentDirectory, "Files", "錢進系統MI操作手冊20241211.pdf");
        var miDoc = new Document().AddFile(miDocPath).AddTag("type", "mi");
        var documentId = await memory.ImportDocumentAsync(miDoc, steps:
            [
                Constants.PipelineStepsExtract,
                "summarize", // alternative to default "summarize", 55secs vs 50secs
                Constants.PipelineStepsGenEmbeddings,
                Constants.PipelineStepsSaveRecords
            ]);
        //var documentId = "be3553bfb1b2450caf3587870c113a11202504090406574503133";

        if (!await memory.IsDocumentReadyAsync(documentId: documentId))
            Console.WriteLine("文件尚未準備好。");
        else
            Console.WriteLine("文件已準備好。");

        ////// 驗證匯入的內容
        //var documentContent = await memory.ExportFileAsync(documentId, "錢進系統MI操作手冊20241211.pdf");
        //if (documentContent == null)
        //{
        //    Console.WriteLine("匯入的文檔內容為空。");
        //}
        //else
        //{
        //    //將 documentContent 從資料留讀出並顯示文字
        //    var streams = await documentContent.GetStreamAsync();

        //    //將 Stream 轉 byte 後轉字串
        //    var bytes = new byte[streams.Length];
        //    await streams.ReadAsync(bytes, 0, (int)streams.Length);
        //    var text = Encoding.UTF8.GetString(bytes);
        //    Console.WriteLine("檔案內容: {0}", text);
        //}

        //var dataPipelineStatus = await memory.GetDocumentStatusAsync(documentId);
        //Console.WriteLine($"dataPipelineStatus: {System.Text.Json.JsonSerializer.Serialize(dataPipelineStatus)}");

        //SearchResult relevant = await memory.SearchAsync(
        //        query: question2,
        //        minRelevance: 0,
        //        limit: -1,
        //        filter: MemoryFilters.ByDocument(documentId).ByTag("type", "mi")
        //    );

        //foreach (Citation result in relevant.Results)
        //{
        //    // Store the document IDs so we can load all their records later
        //    Console.WriteLine($"Document ID: {result.DocumentId}");
        //    Console.WriteLine($"Relevant partitions: {result.Partitions.Count}");
        //    foreach (Citation.Partition partition in result.Partitions)
        //    {
        //        Console.WriteLine($" * Partition {partition.PartitionNumber}, relevance: {partition.Relevance} text: {partition.Text}");
        //    }

        //    Console.WriteLine("--------------------------");
        //}

        var result = await memory.SearchSyntheticsAsync("summary", filter: MemoryFilters.ByDocument(documentId));
        foreach (Citation citation in result)
        {
            Console.WriteLine($"== {citation.SourceName} summary ==\n{citation.Partitions.First().Text}\n");
        }

        //        var question2 = "請問 MoneyIn 操作文件中，該系統功能有哪些，請給我功能名稱並用「,」區隔提供給我";
        //        Console.WriteLine($"question2 - {question2}");

        //        var myPrompt = @"
        //Given the following context, answer the user's question:

        //Context:
        //{{ $input }}

        //Question:
        //{{ $query }}

        //Answer:
        //";

        //        var answer2 = await memory.AskAsync(question2, filter: MemoryFilters.ByDocument(documentId));
        //        Console.WriteLine($"\nAnswer2: {answer2.Result}");

        //        var answer2ResultArr = answer2.Result.Split(',');

        //foreach (var item in answer2ResultArr)
        //{
        //    var question3 = $"幫我針對這份系統的單元「{item}」整理 Q/A 清單，問題與答案需有條理且正確，字數限制給我最少 10 題，最多 50 題";
        //    Console.WriteLine($"question3 - {item}");

        //    var answer3 = await memory.AskAsync(question3, filter: MemoryFilters.ByDocument(documentId));
        //    Console.WriteLine($"\nAnswer3 - {item}: {answer3.Result}");
        //}

        #endregion
    }
}
