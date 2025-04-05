using KernelSample.KernelMemory;
using KernelSample.OpenAI;
using KernelSample.Qdrant;

//string apikey = Environment.GetEnvironmentVariable("AI:OpenAI:APIKey")!;
string apiKey = "<api-key>";

// Initialize the kernel
//await new OpenAISmapleOne().RunAsync(apiKey);
await new ImportHotalDataForQdrantSample2().RunAsync(apiKey);


//Console.WriteLine(JsonSerializer.Serialize(new WidgetDetails()
//{
//    Colors = new[]
//    {
//        WidgetColor.Green
//    },
//    SerialNumber = "1",
//    Type = WidgetType.Decorative
//}));
