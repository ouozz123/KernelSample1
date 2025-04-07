namespace KernelSample.Qdrant.Model;

using Microsoft.Extensions.VectorData;

public sealed class Hotel2
{
    /// <summary>
    /// 旅館代碼
    /// </summary>
    /// <remarks>
    /// 關鍵屬性必須是受支援的類型之一：System.UInt64，System.Guid
    /// </remarks>
    [VectorStoreRecordKey]
    public ulong HotelId { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [VectorStoreRecordData]
    public required string Address { get; set; }

    /// <summary>
    /// 旅館名稱
    /// </summary>
    [VectorStoreRecordData]
    public required string HotelName { get; set; }

    /// <summary>
    /// 旅館描述
    /// </summary>
    /// <remarks>
    /// 有茶民宿位於南投縣竹山鎮中山里25鄰文化路5號，類別為民宿
    /// </remarks>
    [VectorStoreRecordData(IsFullTextSearchable = true)]
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// 旅館描述向量
    /// </summary>
    [VectorStoreRecordVector(1536, DistanceFunction.CosineSimilarity, IndexKind.Hnsw)]
    //[VectorStoreRecordVector(1536)]
    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }
}
