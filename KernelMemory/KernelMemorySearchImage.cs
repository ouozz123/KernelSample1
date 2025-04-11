using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DataFormats.AzureAIDocIntel;
using Microsoft.KernelMemory.DataFormats.Image;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using System.Text;

namespace KernelSample.KernelMemory;
internal class KernelMemorySearchImage : Sample
{
    private const string QdrantServiceId = "QdrantServiceId";

    internal override async Task RunAsync(string apiKey)
    {
        //https://portal.azure.com/#@promise52252gmail.onmicrosoft.com/resource/subscriptions/e83f0d2c-ed37-49d2-872f-8830e8ae7288/resourceGroups/test/providers/Microsoft.CognitiveServices/accounts/kerneltest/overview
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
                .AddConsole((x) => {})
                .SetMinimumLevel(LogLevel.Debug); // 設置最低日誌級別
        });

        #endregion

        var azureAIDocIntelConfig = new AzureAIDocIntelConfig()
        {
            APIKey = "3xrcNo0IDi5gfvHhoGSZQg362Vs0MjxxDwhzHwjdkKAVO3Zb63yxJQQJ99BDACYeBjFXJ3w3AAALACOGzh8P",
            Auth = AzureAIDocIntelConfig.AuthTypes.APIKey,
            Endpoint = "https://kerneltest.cognitiveservices.azure.com/"
        };
#pragma warning disable KMEXP02 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var ocrEngine = new AzureAIDocIntelEngine(azureAIDocIntelConfig, loggerFactory);
#pragma warning restore KMEXP02 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
#pragma warning disable KMEXP00 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var imageDecoder = new ImageDecoder(ocrEngine, loggerFactory);
#pragma warning restore KMEXP00 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

        var memory = new KernelMemoryBuilder()
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
            .WithContentDecoder(imageDecoder) // Register a custom Word decoder
            .Build<MemoryServerless>();

        //#pragma warning disable KMEXP02 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        //        var ocrEngine = new AzureAIDocIntelEngine(azureAIDocIntelConfig, loggerFactory);
        //#pragma warning restore KMEXP02 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        //#pragma warning disable KMEXP00 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        //        var decoders = new List<IContentDecoder>
        //                {
        //                    //new TextDecoder(loggerFactory),
        //                    //new HtmlDecoder(loggerFactory),
        //                    //new MarkDownDecoder(loggerFactory),
        //                    //new PdfDecoder(loggerFactory),
        //                    //new MsWordDecoder(loggerFactory),
        //                    //new MsExcelDecoder(msExcelDecoderConfig, loggerFactory),
        //                    //new MsPowerPointDecoder(msPowerPointDecoderConfig, loggerFactory),
        //                    new ImageDecoder(ocrEngine, loggerFactory),
        //                };


        //#pragma warning restore KMEXP00 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

        #region 針對圖片

        var image = new Document()
            .AddFile(Path.Combine(Environment.CurrentDirectory, "Files", "錢進系統icon.png"))
            .AddTag("type", "image");
        var imageId = await memory.ImportDocumentAsync(image);

        var askResult = await memory.AskAsync("這個圖片背景顏色?", filter: MemoryFilters.ByDocument(imageId));
        Console.WriteLine("imageResult: {0}", askResult);
        #endregion
    }
}