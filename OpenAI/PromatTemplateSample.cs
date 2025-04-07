using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;


namespace KernelSample.OpenAI;
internal class PromatTemplateSample : Sample
{
    internal override async Task RunAsync(string apiKey)
    {
        var builder = Kernel.CreateBuilder();
        builder
            // For OpenAI:
            .AddOpenAIChatCompletion(
                modelId: "gpt-4o-mini",
                apiKey: apiKey,
                httpClient: HttpLogger.GetHttpClient(true));

        //溫度 (預設1):使用什麼採樣溫度，在 0 到 2 之間。較高的值（如 0.8）將使輸出更加隨機，而較低的值（如 0.2）將使其更加集中和確定性。我們通常建議改變這一點top_p，但不要同時改變兩者。
        var promptOptions = new OpenAIPromptExecutionSettings { ChatSystemPrompt = "Answer or say \"I don't know\".", MaxTokens = 16384, Temperature = 0, TopP = 0 };

        var kernel = builder.Build();

        //註冊 memory plugin
        var memoryConnector = GetMemoryConnector();
        var memoryPlugin = kernel.ImportPluginFromObject(new MemoryPlugin(memoryConnector, waitForIngestionToComplete: true), "memory");

        ////新增檔案至 memory plugin
        //string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        //string filePath = Path.Combine(projectDirectory, "Files", "旅宿列表匯出_20250401113015.xlsx");
        //var context = new KernelArguments
        //{
        //    [MemoryPlugin.FilePathParam] = filePath,
        //    [MemoryPlugin.DocumentIdParam] = "hotel-list-01"
        //};
        //await memoryPlugin["SaveFile"].InvokeAsync(kernel, context);

        const string Question1 = "請問在台灣北部有哪些民宿擁有「好客民宿」的標章?";
        var context = new KernelArguments
        {
            ["input"] = "{{$input}}",
            [MemoryPlugin.DocumentIdParam] = "hotel-list-01"
        };
        await memoryPlugin["Save"].InvokeAsync(kernel, context);



        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        string filePath = Path.Combine(projectDirectory, "Files", "旅宿列表匯出_20250401113015_part1.txt");
        string text = File.ReadAllText(filePath);   

        //var skPrompt = """
        //               問題: {{$input}}
        //               擁有的資料源: {{memory.ask $memoryInput documentId="hotel-list-01"}}
        //               限制: 根據「擁有的資料源」尋找問題的答案，如何找不到答案就說我不知道
        //               """;
        var skPrompt = """
                       問題: {{$input}}
                       擁有的旅館csv檔案資料: {{$memoryInput}}
                       限制: 根據「擁有的旅館csv檔案資料」尋找問題的答案，如何找不到答案就說我不知道
                       """;

        var memoryInput = """
            整理所有的旅館資訊給我，不限制字數，
            """;
        var myFunction = kernel.CreateFunctionFromPrompt(skPrompt, promptOptions);
        var answer = await myFunction.InvokeAsync(kernel, new KernelArguments()
        {
            ["input"] = Question1,
            ["memoryInput"] = text
        });
        Console.WriteLine("Answer: " + answer);



    //    var settings = new OpenAIPromptExecutionSettings
    //    {
    //        Temperature = 0,
    //        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    //    };

        //    Console.WriteLine(await kernel.InvokePromptAsync<string>(
        //  """
        //            <message role="system">
        //            你的任務是協助使用者，到 kernel_memory search 相關的資訊，並且依據 search result 為基礎，回覆使用者提出的 Question。
        //            若你無法回答請直接回答 "我不知道!"。

        //            </message>
        //            <message role="user">

        //            # Question
        //            {{$question}}

        //            # Answer

        //            </message>
        //            """,
        //          new(settings)
        //          {
        //              ["question"] = Question1
        //          }));



        //    //var answer2 = await kernel.InvokePromptAsync(Question1);
        //    //Console.WriteLine("Answer2: " + answer2);
      }

    private static IKernelMemory GetMemoryConnector(bool serverless = false)
    {
        if (!serverless)
        {
            return new MemoryWebClient("http://127.0.0.1:9001/");
            //return new MemoryWebClient("http://127.0.0.1:9001/", Environment.GetEnvironmentVariable("MEMORY_API_KEY"));
        }

        Console.WriteLine("This code is intentionally disabled.");
        Console.WriteLine("To test the plugin with Serverless memory:");
        Console.WriteLine("* Add a project reference to CoreLib");
        Console.WriteLine("* Uncomment/edit the code in " + nameof(GetMemoryConnector));
        Environment.Exit(-1);
        return null;

        // return new KernelMemoryBuilder()
        //     .WithAzureOpenAIEmbeddingGeneration(new AzureOpenAIConfig
        //     {
        //         APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
        //         Endpoint = EnvVar("AOAI_ENDPOINT"),
        //         Deployment = EnvVar("AOAI_DEPLOYMENT_EMBEDDING"),
        //         Auth = AzureOpenAIConfig.AuthTypes.APIKey,
        //         APIKey = EnvVar("AOAI_API_KEY"),
        //     })
        //     .WithAzureOpenAITextGeneration(new AzureOpenAIConfig
        //     {
        //         APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
        //         Endpoint = EnvVar("AOAI_ENDPOINT"),
        //         Deployment = EnvVar("AOAI_DEPLOYMENT_TEXT"),
        //         Auth = AzureOpenAIConfig.AuthTypes.APIKey,
        //         APIKey = EnvVar("AOAI_API_KEY"),
        //     })
        //     .Build<MemoryServerless>();
    }
}
