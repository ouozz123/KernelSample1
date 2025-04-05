using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace KernelSample.OpenAI;
internal class OpenAISample4 : Sample
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
        StringBuilder builder = new();

        // Q&A loop
        while (true)
        {
            Console.Write("Question: ");
            chat.AddUserMessage(Console.ReadLine()!);

            builder.Clear();

            //需要即時顯示 AI 逐步輸出的回應，如聊天機器人
            await foreach (StreamingChatMessageContent message in ai.GetStreamingChatMessageContentsAsync(chat))
            {
                Console.Write(message);
                builder.Append(message.Content);
            }
            Console.WriteLine();
            chat.AddAssistantMessage(builder.ToString());

            Console.WriteLine();
        }
    }
}
