using KernelSample.KernelMemory;
using KernelSample.OpenAI;
using KernelSample.Qdrant;

//string apikey = Environment.GetEnvironmentVariable("AI:OpenAI:APIKey")!;
string apiKey = "<api-key>";

//將 apiKey 紹定至環境變數
Environment.SetEnvironmentVariable("OPENAI_API_KEY", apiKey);

// 驗證環境變數是否設定成功
string? retrievedApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (retrievedApiKey == apiKey)
    Console.WriteLine("環境變數設定成功！");
else
{
    Console.WriteLine("環境變數設定失敗！");
    return;
}

// Initialize the kernel
//await new OpenAISmapleOne().RunAsync(apiKey);
//await new SearchQdrantByKernelMemory().RunAsync(apiKey);
//await new HotelPlugnSample().RunAsync(apiKey);
// var textModel = "gpt-4o";
// var textModel = "gpt-4o-mini";
// var embeddingModel = "text-embedding-3-small";
// var fileName = Path.Combine(Environment.CurrentDirectory, "Files", "錢進系統MI操作手冊20241211.md");
// await new GenDocQA().RunAsync(textModel, embeddingModel, fileName);


//Console.WriteLine(JsonSerializer.Serialize(new WidgetDetails()
//{
//    Colors = new[]
//    {
//        WidgetColor.Green
//    },
//    SerialNumber = "1",
//    Type = WidgetType.Decorative
//}));

await new ReadExcelContent().RunAsync();