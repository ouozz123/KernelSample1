using KernelSample.Qdrant.Model;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Data;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Collections.Generic;
using System.Threading;

namespace KernelSample.Qdrant;

internal class VectorStoreTextSearchSample : Sample
{
    internal override async Task RunAsync(string apiKey)
    {
        // Create an embedding generation service.
#pragma warning disable SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var textEmbeddingGeneration = new OpenAITextEmbeddingGenerationService(
                modelId: "text-embedding-3-small",
                apiKey: apiKey);
#pragma warning restore SKEXP0010 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

        var vectorStore = new QdrantVectorStore(new QdrantClient("localhost"));

        var collectionName = "skhotels";

        // 如果不存在並創建收集
        var recordCollection = vectorStore.GetCollection<ulong, Hotel>(collectionName);
        await recordCollection.CreateCollectionIfNotExistsAsync().ConfigureAwait(false);

        // TODO populate the record collection with your test data
        // Example https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Search/VectorStore_TextSearch.cs

        // Create custom mapper to map a <see cref="DataModel"/> to a <see cref="string"/>
        var stringMapper = new HotelTextSearchStringMapper();

        // Create custom mapper to map a <see cref="DataModel"/> to a <see cref="TextSearchResult"/>
        var resultMapper = new HotelTextSearchResultMapper();

        // 使用內存矢量存儲創建文本搜索實例
        var textSearch = new VectorStoreTextSearch<Hotel>(recordCollection, textEmbeddingGeneration, stringMapper, resultMapper);

        // 搜索和返回結果作為文本搜索結果項目
        var query = "給我天堂鳥相關的資訊";
        KernelSearchResults<TextSearchResult> textResults = await textSearch.GetTextSearchResultsAsync(query, new() { Top = 2, Skip = 0 });
        Console.WriteLine("\n--- Text Search Results ---\n");
        await foreach (TextSearchResult result in textResults.Results)
        {
            Console.WriteLine($"Name:  {result.Name}");
            Console.WriteLine($"Value: {result.Value}");
            Console.WriteLine($"Link:  {result.Link}");
        }

       // Unhandled exception. System.InvalidOperationException: Value property of KernelSample.Qdrant.Model.Hotel cannot be null.
       //at Microsoft.SemanticKernel.Data.VectorStoreTextSearch`1.< CreateTextSearchResultMapper > b__13_0(Object result)
       //at Microsoft.SemanticKernel.Data.TextSearchResultMapper.MapFromResultToTextSearchResult(Object result)
       //at Microsoft.SemanticKernel.Data.VectorStoreTextSearch`1.GetResultsAsTextSearchResultAsync(IAsyncEnumerable`1 searchResponse, CancellationToken cancellationToken) + MoveNext()
       //at Microsoft.SemanticKernel.Data.VectorStoreTextSearch`1.GetResultsAsTextSearchResultAsync(IAsyncEnumerable`1 searchResponse, CancellationToken cancellationToken) + MoveNext()
       //at Microsoft.SemanticKernel.Data.VectorStoreTextSearch`1.GetResultsAsTextSearchResultAsync(IAsyncEnumerable`1 searchResponse, CancellationToken cancellationToken) + System.Threading.Tasks.Sources.IValueTaskSource<System.Boolean>.GetResult()
       //at KernelSample.Qdrant.VectorStoreTextSearchSample.RunAsync(String apiKey) in D:\KernelSample\Qdrant\VectorStoreTextSearchSample.cs:line 38
       //at KernelSample.Qdrant.VectorStoreTextSearchSample.RunAsync(String apiKey) in D:\KernelSample\Qdrant\VectorStoreTextSearchSample.cs:line 38
       //at Program.< Main >$(String[] args) in D:\KernelSample\Program.cs:line 8
       //at Program.< Main > (String[] args)
    }
}
