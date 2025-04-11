using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace KernelSample.KernelMemory;
internal class ImportDocToQdrantDemo
{
    internal async Task RunAsync(string textModel, string embeddingModel, string fileName)
    {
        var openAIConfig = new OpenAIConfig()
        {
            APIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!,
            TextModel = textModel,
            EmbeddingModel = embeddingModel,
            TextModelMaxTokenTotal = 16384,
            EmbeddingModelMaxTokenTotal = 8191,
        };
        var httpClient = HttpLogger.GetHttpClient(true);

        var builder = Kernel.CreateBuilder();

        var kernelMemory = new KernelMemoryBuilder()
            .WithQdrantMemoryDb("http://127.0.0.1:6333")
            //.WithCustomTextPartitioningOptions(new Microsoft.KernelMemory.Configuration.TextPartitioningOptions()
            //{
            //    MaxTokensPerParagraph = 100,
            //    //塊之間的重疊令牌的數量
            //    OverlappingTokens = 50,
            //})
            .WithOpenAITextGeneration(openAIConfig, httpClient: httpClient)
            .WithOpenAITextEmbeddingGeneration(openAIConfig, httpClient: httpClient)
            .Build<MemoryServerless>(new KernelMemoryBuilderBuildOptions()
            {
                //允許混合揮發性和持久數據
                AllowMixingVolatileAndPersistentData = true
            });
    }
}
