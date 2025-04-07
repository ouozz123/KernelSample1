using DocumentFormat.OpenXml.Drawing.Diagrams;
using KernelSample.Qdrant.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using MongoDB.Bson;
using System.ComponentModel;

namespace KernelSample.Plugin.HotelPlugin;
public class HotelPlugin
{
    //private readonly IVectorStore store;
    //private readonly ITextEmbeddingGenerationService embeddingService;
    private readonly ISemanticTextMemory memory;

    public HotelPlugin(
        //[FromKeyedServices("QdrantStore")] IVectorStore store, 
        //ITextEmbeddingGenerationService embeddingService
        ISemanticTextMemory memory
        )
    {
        this.memory = memory;
        //this.store = store;
        //this.embeddingService = embeddingService;
    }

    [KernelFunction("SearchHotel")]
    [Description("根據用戶問題獲取相關的酒店信息")]
    public async Task<List<Hotel2>> SearchHotel(string keyword)
    {
        //函數名稱只能包含 ASCII 字母、數字和底線：「取得飯店資訊」不是有效名稱。 (參數‘函數名稱’)’
        var searchResult =  memory.SearchAsync("hotel2", keyword, minRelevanceScore: 0.3);

        var result = new List<Hotel2>();
        await foreach (var item in searchResult)
        {
            Console.WriteLine(item.ToJson());
            //result.Add(new Hotel2() { HotelName = item.Metadata. })
        }      

        return new List<Hotel2>()
        {
            new Hotel2()
            {
                HotelName = "澎湖旅館",
                Address = "澎湖縣馬公市中正路1號",
                Description = "澎湖旅館位於澎湖縣馬公市中正路1號，類別為旅館",
            }
        };
    }
}
