using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;

namespace KernelSample.KernelMemory;
internal class KernelWithKernelMemoryPlugin : Sample
{
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

        var builder = Kernel.CreateBuilder();
        builder
            // For OpenAI:
            .AddOpenAIChatCompletion(
                modelId: textModel,
                apiKey: EnvVar("OPENAI_API_KEY"),
                httpClient: httpClient);


        var kernel = builder.Build();

        var promptOptions = new OpenAIPromptExecutionSettings { 
            ChatSystemPrompt = "Answer or say \"I don't know\".", 
            MaxTokens = 100, 
            Temperature = 0, 
            TopP = 0 
        };

        var skPrompt = """
                       Question: {{$input}}
                       Tool call result: {{memory.ask $input}}
                       請參考 Tool call result 給我答案
                       """;

        var myFunction = kernel.CreateFunctionFromPrompt(skPrompt, promptOptions);

        skPrompt = """
                   Question: {{$input}}
                   Tool call result: {{memory.ask $input index='private'}}
                   請參考 Tool call result 給我答案
                   """;

        var myFunction2 = kernel.CreateFunctionFromPrompt(skPrompt, promptOptions);



        #region 建立 logger
        Console.OutputEncoding = Encoding.UTF8;

        // 建立 LoggerFactory 實例
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole((x) =>
                {
                })
                .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug); // 設置最低日誌級別
        });

        #endregion
        var memoryConnector = GetMemoryConnector(openAIConfig, httpClient, true);
        var memoryPlugin = kernel.ImportPluginFromObject(new MemoryPlugin(memoryConnector, waitForIngestionToComplete: true), "memory");

        var fileName = Path.Combine(Environment.CurrentDirectory, "Files", "錢進系統MI操作手冊20241211.md");

        //var context = new KernelArguments
        //{
        //    [MemoryPlugin.FilePathParam] = fileName,
        //    [MemoryPlugin.DocumentIdParam] = "doc1"
        //};
        //await memoryPlugin["SaveFile"].InvokeAsync(kernel, context);

        ////儲存文字
        var context = new KernelArguments
        {
            ["index"] = "private",
            ["input"] = "我的爸爸名字叫郭台銘",
            [MemoryPlugin.DocumentIdParam] = "PRIVATE01"
        };
        await memoryPlugin["Save"].InvokeAsync(kernel, context);


        Console.WriteLine("---------");
        //var question1 = "請問錢進系統的操作手冊是什麼？";
        //Console.WriteLine("Question1: " + question1);
        //var answer1 = await myFunction.InvokeAsync(kernel, question1);
        //Console.WriteLine("Answer1: " + answer1);

        var question2 = "我的爸爸叫甚麼名字？";
        Console.WriteLine("Question2: " + question2);
        var answer2 = await myFunction2.InvokeAsync(kernel, question2);
        Console.WriteLine("Answer2: " + answer2);

    }

    private static IKernelMemory GetMemoryConnector(OpenAIConfig openAIConfig, HttpClient httpClient, bool serverless = false)
    {
        if (!serverless)
        {
            //未實作拋例外
            throw new NotImplementedException("MemoryServerless is not implemented yet");
            //return new MemoryWebClient("http://127.0.0.1:9001/", Environment.GetEnvironmentVariable("MEMORY_API_KEY"));
        }
        else
        {
            return new KernelMemoryBuilder()
                //設定 memory 回應的 c
                .WithCustomTextPartitioningOptions(new Microsoft.KernelMemory.Configuration.TextPartitioningOptions()
                {
                    MaxTokensPerParagraph = 100,
                    //塊之間的重疊令牌的數量
                    OverlappingTokens = 50,
                })
                .WithOpenAITextGeneration(openAIConfig, httpClient: httpClient)
                .WithOpenAITextEmbeddingGeneration(openAIConfig, httpClient: httpClient)
                .Build<MemoryServerless>();
        }
    }

    private static string EnvVar(string name)
    {
        return Environment.GetEnvironmentVariable(name)
               ?? throw new ArgumentException($"Env var {name} not set");
    }
}
