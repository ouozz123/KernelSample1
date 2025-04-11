using DnsClient.Internal;
using DocumentFormat.OpenXml.Drawing;
using Elastic.Clients.Elasticsearch;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using KernelSample.Extensions;
using KernelSample.Qdrant.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Text.Json.Serialization.Metadata;

namespace KernelSample.Qdrant
{
    /// <summary>
    /// 匯入多個文件至 Qdrant，並使用 Qdrant 進行搜尋
    /// </summary>
    public class ImportMultipleDocToSearchWithQdrant
    {
        private static ITextEmbeddingGenerationService textEmbeddingGenerationService;
        private async Task<List<MoneyInDocs>> ReadAndEmbedDocumentsAsync(string directoryPath)
        {
            var models = await ReadDicrectoryFilesAsync(directoryPath);

            foreach (var text in models)
            {
                text.ContentEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(text.Content);
            }

            return models;
        }

        private async Task SetupQdrantCollectionAsync(QdrantVectorStore vectorStore, string collectionName, List<MoneyInDocs> models)
        {
            var moneyInDocCollection = vectorStore.GetCollection<ulong, MoneyInDocs>(collectionName);

            // 建立 Qdrant旅館 Table
            await moneyInDocCollection.DeleteCollectionAsync();
            await moneyInDocCollection.CreateCollectionIfNotExistsAsync();

            //建立旅館資料
            await foreach (var result in moneyInDocCollection.UpsertBatchAsync(models))
            {
                Console.WriteLine($"成功更新或插入的 ID：{result}");
            }
        }

        internal async Task RunAsync(bool executeReadAndEmbed = false, bool executeSetupQdrant = false)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            var dimensions = 1536; // 向量的維度,原本是128
            var httpClient = HttpLogger.GetHttpClient(true);
            var embeddingModelName = "text-embedding-3-small";
            var aiModel = "gpt-4o-mini";
            var loggerFactory = LoggerFactoryHelper.CreateLoggerFactory();
            var logger = loggerFactory.CreateLogger<ImportMultipleDocToSearchWithQdrant>();

#pragma warning disable SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
            var kernel = Kernel
                .CreateBuilder()
                .AddOpenAIChatCompletion(aiModel, apiKey, httpClient: httpClient)
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
            textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

            List<MoneyInDocs> models = null;
            if (executeReadAndEmbed)
            {
                var directoryPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Files", "mi_doc");
                models = await ReadAndEmbedDocumentsAsync(directoryPath);
            }

            var channel = GrpcChannel.ForAddress("http://localhost:6334", new GrpcChannelOptions
            {
                //HttpClient = httpClient,
                //LoggerFactory = loggerFactory,
            }).Intercept(new LoggingInterceptor(loggerFactory));

            var qdrantGrpcClient = new QdrantGrpcClient(channel);
            var qdrantClient = new QdrantClient(qdrantGrpcClient);
            var vectorStore = new QdrantVectorStore(qdrantClient, new QdrantVectorStoreOptions() { HasNamedVectors = false });
            var hotelCollectionName = "MoneyInDocs";

            if (executeSetupQdrant && models != null)
            {
                await SetupQdrantCollectionAsync(vectorStore, hotelCollectionName, models);
            }

            var moneyInDocCollection = vectorStore.GetCollection<ulong, MoneyInDocs>(hotelCollectionName);
            var searchQuery = "客戶查詢功能有哪些?";
            var searchResult = await SearchAsync(vectorStore, hotelCollectionName, searchQuery);

            var vectorDocs = new List<VectorSearchResult<MoneyInDocs>>();
            await foreach  (var result in searchResult.Results)
            {
                if(result.Score > 0.6)
                    vectorDocs.Add(result);
                //Console.WriteLine("{0}", System.Text.Json.JsonSerializer.Serialize(result.Record, options: new()
                //{
                //    WriteIndented = true
                //}));
                //Console.WriteLine("Score: {0}", result.Score);
            }

            var docContents = string.Join(Environment.NewLine, vectorDocs.Select(x => x.Record.Content));

            var prompt = """
                問題:{{$input}}
                操作文件內容:{{$docContents}}
                如果操作文件內容為空白，請回答「我不知道」。
                """;

            var promptOptions = new OpenAIPromptExecutionSettings
            {
                ChatSystemPrompt = "你現在是一位MoneyIn系統專家，目前有操作人員反應問題，請根據操作文件內容提供說明，說明需簡潔有力",
                MaxTokens = 5000,
                //溫度控製完成的隨機性。溫度越高完成越隨意。默認值為1.0。
                Temperature = 0,
                //頂部p越高，完成越多樣化
                TopP = 0
            };
            var function = kernel.CreateFunctionFromPrompt(prompt, promptOptions);  

            var answer = await function.InvokeAsync(kernel, new KernelArguments
            {
                ["input"] = "可以介紹一下客服查詢功能嗎?",
                ["docContents"] = docContents
            });

            Console.WriteLine("Answer: {0}", answer);

            // Answer: 客服查詢功能是MoneyIn系統中一個非常實用的模組，主要用於快速查找和檢視客戶資料。以下是該功能的主要特點和操作說明：

            //客服查詢功能是用來快速查找並檢視客戶資料的模組，操作人員可以依據客戶編號、電話、身分證字號、姓名或電子信箱等條件進行查詢。查詢結果會顯示客戶的基本資料，包括手機、地址、信箱、會員等級與維運專員等資訊。此外，查詢頁面右側提供功能按 鈕，讓專員可以執行訂單查詢、聯繫歷史、活動管理、票券操作等作業。
            //這個功能不僅能提高專員的工作效率，還能幫助專員更有效地推廣對應的商品，進而提高成交率。例如，專員可以透過交談註記搜尋特定關鍵字，快速找到相關的客戶聯繫內容和交談紀錄。
            //總之，客服查詢功能是提升客戶服務質量和效率的重要工具。
        }


        /// <summary>
        /// 對 qdrant 進行搜尋
        /// </summary>
        /// <param name="store"></param>
        /// <param name="hotelCollectionName"></param>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        /// <remarks>
        /// 會取得到他搜尋的資料結果
        /// </remarks>
        private static async Task<VectorSearchResults<MoneyInDocs>> SearchAsync(IVectorStore store, string hotelCollectionName, string searchQuery){
           
            var moneyInDocCollection = store.GetCollection<ulong, MoneyInDocs>(hotelCollectionName);
            var searchVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync(searchQuery);
            var searchResult = await moneyInDocCollection.VectorizedSearchAsync(searchVector, options: new()
            {
                Filter = x => x.FileName == "錢進系統MI操作手冊20241211.md",
                Top = 10,
            });
            return searchResult;
        }

        private static async Task<List<MoneyInDocs>> ReadDicrectoryFilesAsync(string dicrectoryPath)
        {
            //讀取資料夾中的所有 md 檔案內容至
            var files = Directory.GetFiles(dicrectoryPath, "*.md", SearchOption.AllDirectories);
            var markdownFiles = new List<MoneyInDocs>();

            ulong i = 1;
            foreach (var file in files)
            {
                var lines = await ReadFileLinesAsync(file);
                markdownFiles.Add(new MoneyInDocs()
                {
                    Id = i++,
                    Index = file.Split("_").Last().Split(".").First().ToInt(),
                    //FileName = System.IO.Path.GetFileName(file),
                    FileName = "錢進系統MI操作手冊20241211.md",
                    Content = string.Join(Environment.NewLine, lines)
                });
                Console.WriteLine("檔案名稱: {0}", file);
            }

            return markdownFiles;
        }

        private static async Task<List<string>> ReadFileLinesAsync(string fileName)
        {
            //取得專案
            string projectDirectory = Directory.GetCurrentDirectory();
            string filePath = System.IO.Path.Combine(projectDirectory, "Files", fileName);
            // Console.WriteLine("filePath:{0}", filePath);

            List<string> lines = new List<string>();
            if (File.Exists(filePath))
            {
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
            using var streamReader = new StreamReader(filePath, System.Text.Encoding.UTF8);
            while (!streamReader.EndOfStream)
                yield return await streamReader.ReadLineAsync();
        }

        public class MarkdownFile
        {
            public int Index { get; set; }

            public string FileName { get; set; }

            public string Content { get; set; }
        }
    }
}