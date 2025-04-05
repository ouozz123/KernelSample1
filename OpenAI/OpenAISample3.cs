using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace KernelSample.OpenAI;
internal class OpenAISample3 : Sample
{
    internal override async Task RunAsync(string apiKey)
    {
        // Initialize the kernel
        Kernel kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-3.5-turbo-0125", apiKey)
            .Build();

        // Create a new chat
        IChatCompletionService ai = kernel.GetRequiredService<IChatCompletionService>();
        ChatHistory chat = new("You are an AI assistant that helps people find information.");

        // Q&A loop
        while (true)
        {
            Console.Write("Question: ");
            chat.AddUserMessage(Console.ReadLine()!);

            //需要完整回應，適合短回覆或同步處理
            var answer = await ai.GetChatMessageContentAsync(chat);
            chat.AddAssistantMessage(answer.Content!);
            Console.WriteLine(answer);

            Console.WriteLine();
        }
    }
}
