using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Numerics.Tensors;

namespace KernelSample.OpenAI;
internal class OpenAISample5 : Sample
{
    internal override async Task RunAsync(string apiKey)
    {
#pragma warning disable SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var embeddingGen = new OpenAITextEmbeddingGenerationService("text-embedding-3-small", apiKey);
#pragma warning restore SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

        string input = "What is an amphibian?";
        string[] examples =
        {
            "What is an amphibian?",
            "Cos'è un anfibio?",
            "A frog is an amphibian.",
            "Frogs, toads, and salamanders are all examples.",
            "Amphibians are four-limbed and ectothermic vertebrates of the class Amphibia.",
            "They are four-limbed and ectothermic vertebrates.",
            "A frog is green.",
            "A tree is green.",
            "It's not easy bein' green.",
            "A dog is a mammal.",
            "A dog is a man's best friend.",
            "You ain't never had a friend like me.",
            "Rachel, Monica, Phoebe, Joey, Chandler, Ross",
        };

        // Generate embeddings for each piece of text
        var inputEmbedding = (await embeddingGen.GenerateEmbeddingsAsync([input]))[0];
        var exampleEmbeddings = await embeddingGen.GenerateEmbeddingsAsync(examples);

        // Print the cosine similarity between the input and each example
        float[] similarity = exampleEmbeddings.Select(e => TensorPrimitives.CosineSimilarity(e.Span, inputEmbedding.Span)).ToArray();
        similarity.AsSpan().Sort(examples.AsSpan(), (f1, f2) => f2.CompareTo(f1));
        Console.WriteLine("Similarity Example");
        for (int i = 0; i < similarity.Length; i++)
            Console.WriteLine($"{similarity[i]:F6}   {examples[i]}");
    }
}
