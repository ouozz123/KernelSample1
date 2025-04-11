using Azure.Search.Documents;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Text;

namespace KernelSample.KernelMemory;


/// <summary>
/// 產生文件的 QA
/// </summary>
internal class GenDocQA
{
    internal async Task RunAsync(string textModel, string embeddingModel, string fileName)
    {
        //初始化
        var openAIConfig = new OpenAIConfig()
        {
            APIKey = EnvVar("OPENAI_API_KEY"),
            TextModel = textModel,
            EmbeddingModel = embeddingModel,
            TextModelMaxTokenTotal = 16384,
            EmbeddingModelMaxTokenTotal = 8191,
            MaxEmbeddingBatchSize = 2048
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

        //初始化 Kernel Memory 插件，並掛載至 Kernel
        var loggerFactory = CreateLoggerFactory();
        var memoryConnector = GetMemoryConnector(openAIConfig, httpClient, true);
        var memoryPlugin = kernel.ImportPluginFromObject(new MemoryPlugin(memoryConnector, waitForIngestionToComplete: true), "memory");

        ////匯入文件內容至 Kernel Memory
        var context = new KernelArguments
        {
            ["index"] = "private",
            [MemoryPlugin.FilePathParam] = fileName,
            [MemoryPlugin.DocumentIdParam] = "doc1"
        };
        await memoryPlugin["SaveFile"].InvokeAsync(kernel, context);


        ////直接對 memory plugin 進行題問
        //var context = new KernelArguments
        //{
        //    [MemoryPlugin.QuestionParam] = "請問怎麼登入，請用中文解釋",
        //    [MemoryPlugin.IndexParam] = "private"
        //};
        //var result = await memoryPlugin["ask"].InvokeAsync(kernel, context);
        //Console.WriteLine("Answer: {0}" , result);


        ////直接對 memory plugin 進行題問
        var context2 = new KernelArguments
        {
            //[MemoryPlugin.QuestionParam] = "請問這份文件有哪些主要功能?",
            [MemoryPlugin.QuestionParam] = "可否介紹訂單客服管理的客戶查詢功能?",
            [MemoryPlugin.IndexParam] = "private"
        };
        var result = await memoryPlugin["ask"].InvokeAsync(kernel, context2);
        Console.WriteLine("Answer: {0}", result);



        ////產生 Q/A 問題清單 50 個
        //var skPrompt = """
        //            Question: {{$input}}
        //            文件內容: 
        //            {{memory.ask $memory_input index='private'}}
        //            請參考文件內容產生各個功能的題目跟答案各5個，格式請依照 json 陣列格式如下:
        //            [
        //                 {
        //                     "unitName": <<功能名稱>>,         
        //                     "question": <<問題>>,
        //                     "answer": <<答案>>
        //                 }

        //            ]
        //            """;

        //var promptOptions = new OpenAIPromptExecutionSettings
        //{
        //    ChatSystemPrompt = "你現在是一位文件整理大師，請依據以下的文件內容，做一份QA問題清單，問題與答案必須是文件內容有提及的",
        //    MaxTokens = 16384,
        //    Temperature = 0,
        //    TopP = 0
        //};
        //var promptFunction = kernel.CreateFunctionFromPrompt(skPrompt, promptOptions);
        //var answer = await promptFunction.InvokeAsync(kernel, new KernelArguments
        //{
        //     { "input", "依各個功能產生QA問題清單各5筆" },
        //     { "memory_input", "請問文件中有哪些主要功能?" }
        //});
        //Console.WriteLine("Answer: " + answer);


        //var skPrompt2 = """
        //           Question: {{$input}}
        //           文件內容: {{memory.ask $input index='private'}}
        //           """;
        //var promptOptions = new OpenAIPromptExecutionSettings
        //    {
        //        ChatSystemPrompt = "你現在是一個 錢進系統 MONEY IN 資深專員，根據使用者的提問並參考文件內容給我正確操作的流程描述，必須是文件內容有的內容",
        //        MaxTokens = 16384,
        //        Temperature = 0,
        //        TopP = 0
        //    };
        //var promptFunction2 = kernel.CreateFunctionFromPrompt(skPrompt2, promptOptions);

        //var question = "請問如何進行商品結帳";
        //var answer = await promptFunction2.InvokeAsync(kernel, question);
        ////answer 參考來源
        //var source = answer.("source");



        //Console.WriteLine("Answer: " + answer);
    }

    private static string EnvVar(string name)
    {
        return Environment.GetEnvironmentVariable(name)
               ?? throw new ArgumentException($"Env var {name} not set");
    }

    private static ILoggerFactory CreateLoggerFactory()
    {
        return LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole((x) =>
                {
                })
                .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug); // 設置最低日誌級別
        });
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
                    MaxTokensPerParagraph = 8191,
                    //塊之間的重疊令牌的數量
                    OverlappingTokens = 50,
                })
                .WithOpenAITextGeneration(openAIConfig, httpClient: httpClient)
                .WithOpenAITextEmbeddingGeneration(openAIConfig, httpClient: httpClient)
                .WithSimpleFileStorage(new SimpleFileStorageConfig { StorageType = FileSystemTypes.Disk })
                .WithSimpleVectorDb(new SimpleVectorDbConfig { StorageType = FileSystemTypes.Disk })
                .Build<MemoryServerless>();
        }
    }
}
