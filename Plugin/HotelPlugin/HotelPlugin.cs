using DocumentFormat.OpenXml.Drawing.Diagrams;
using KernelSample.Qdrant;
using KernelSample.Qdrant.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using System.ComponentModel;

namespace KernelSample.Plugin.HotelPlugin;
public class HotelPlugin
{
    private readonly IVectorStore _store;
    private readonly ITextEmbeddingGenerationService _embeddingService;

    public HotelPlugin(
        [FromKeyedServices(HotelPlugnDemo.QdrantServiceId)] IVectorStore store, 
        ITextEmbeddingGenerationService embeddingService)
    {
        this._store = store;
        this._embeddingService = embeddingService;
    }

    [KernelFunction("SearchHotel")]
    [Description("根據用戶問題獲取相關的 hotel 信息")]
    public async Task<List<Hotel2>> SearchHotel([Description("請給我完整的旅館查詢關鍵字")] string search)
    { 
        //向量搜尋
        var searchEmbedding = await _embeddingService.GenerateEmbeddingAsync(search);

        var hotel2 = _store.GetCollection<ulong, Hotel2>(HotelPlugnDemo.HotelCollectionName);
        var record2 = await hotel2.VectorizedSearchAsync(searchEmbedding, new()
            {
                Top = 3 //只抓三筆
            });

        var hotel2s = new List<Hotel2>();

        if (record2 != null)
            await foreach (var item in record2.Results)
            {
                hotel2s.Add(new Hotel2()
                {
                    HotelName = item.Record.HotelName,
                    Description = item.Record.Description,
                    Address = item.Record.Address,
                });

                Console.WriteLine($"搜尋結果：{item.Record.HotelName} - {item.Record.Description} - {item.Score}");
            }

        return hotel2s;
    }
}
