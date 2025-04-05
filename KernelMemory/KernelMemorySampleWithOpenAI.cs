
using Microsoft.KernelMemory;

namespace KernelSample.KernelMemory;
internal class KernelMemorySampleWithOpenAI : Sample
{
    public KernelMemorySampleWithOpenAI()
    {
    }

    internal override async Task RunAsync(string apiKey)
    {
        var memory = new KernelMemoryBuilder().WithOpenAIDefaults(apiKey).Build<MemoryServerless>();

        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        string filePath = Path.Combine(projectDirectory, "Files", "旅宿列表匯出_20250401113015.xlsx");
        await memory.ImportDocumentAsync(filePath);

        var question = "請問在台灣北部有哪些民宿擁有「好客民宿」的標章?";
        Console.WriteLine("question: {0}", question);

        var answer = await memory.AskAsync(question);
        Console.WriteLine("answer: {0}", answer);
    }
}
