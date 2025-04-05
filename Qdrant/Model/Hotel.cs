namespace KernelSample.Qdrant.Model;

using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

public class Hotel
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
    /// 類別
    /// </summary>
    [VectorStoreRecordData(IsFilterable = true, StoragePropertyName = "type")]
    public string Type { get; set; }

    /// <summary>
    /// 標籤
    /// </summary>
    [VectorStoreRecordData(IsFilterable = true, StoragePropertyName = "label")]
    public string Label { get; set;  }

    /// <summary>
    /// 是否有溫泉標籤
    /// </summary>
    [VectorStoreRecordData(IsFilterable = true, StoragePropertyName = "is_spring_label")]
    public bool IsSpringLabel { get; set; }

    /// <summary>
    /// 縣市
    /// </summary>
    [VectorStoreRecordData(IsFilterable = true, StoragePropertyName = "city")]
    public string CityName { get; set; }

    /// <summary>
    /// 地區
    /// </summary>
    [VectorStoreRecordData(IsFilterable = true, StoragePropertyName = "area")]
    public string Area { get; set; }

    /// <summary>
    /// 郵遞區號
    /// </summary>
    [VectorStoreRecordData(StoragePropertyName = "postal_code")]
    public string PostalCode { get; set; }

    /// <summary>
    /// 電話
    /// </summary>
    [VectorStoreRecordData(StoragePropertyName = "phone")]
    public string Phone { get; set;  }

    /// <summary>
    /// EMAIL
    /// </summary>
    [VectorStoreRecordData(StoragePropertyName = "email")]
    public string Email { get; set;  }

    /// <summary>
    /// 網址
    /// </summary>
    [VectorStoreRecordData(StoragePropertyName = "link")]
    public string Link { get; set; }

    /// <summary>
    /// 營業日期
    /// </summary>
    [VectorStoreRecordData(StoragePropertyName = "opening_date")]
    public string OpeningDate { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [VectorStoreRecordData(StoragePropertyName = "address")]
    public string Address { get; set; }

    /// <summary>
    /// 旅館名稱
    /// </summary>
    [VectorStoreRecordData(IsFilterable = true, StoragePropertyName = "hotel_name")]
    public string HotelName { get; set; }

    /// <summary>
    /// 旅館描述
    /// </summary>
    /// <remarks>
    /// 有茶民宿位於南投縣竹山鎮中山里25鄰文化路5號，類別為民宿
    /// </remarks>
    [VectorStoreRecordData(IsFullTextSearchable = true, StoragePropertyName = "hotel_description")]
    public string Description {
        get
        {
            var content = $"{HotelName}位於{Address}，類別為{this.Type}";

            if (this.IsSpringLabel)
                content += "，擁有溫泉標章";

            if (!string.IsNullOrWhiteSpace(this.Label))
                content += $"，擁有{this.Label}";


            return content;
        } 
    }

    /// <summary>
    /// 旅館描述向量
    /// </summary>
    [VectorStoreRecordVector(1536, DistanceFunction.CosineSimilarity, IndexKind.Hnsw, StoragePropertyName = "hotel_description_embedding")]
    //[VectorStoreRecordVector(1536)]
    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }
}

/// <summary>
/// String mapper which converts a Hotel to a string.
/// </summary>
public sealed class HotelTextSearchStringMapper : ITextSearchStringMapper
{
    /// <inheritdoc />
    public string MapFromResultToString(object result)
    {
        if (result is Hotel dataModel)
        {
            return dataModel.HotelName;
        }
        throw new ArgumentException("Invalid result type.");
    }
}

/// <summary>
/// Result mapper which converts a Hotel to a TextSearchResult.
/// </summary>
public sealed class HotelTextSearchResultMapper : ITextSearchResultMapper
{
    /// <inheritdoc />
    public TextSearchResult MapFromResultToTextSearchResult(object result)
    {
        if (result is Hotel dataModel)
        {
            return new TextSearchResult(value: dataModel.HotelName) { Name = dataModel.HotelName.ToString(), Value = dataModel.Description };
        }
        throw new ArgumentException("Invalid result type.");
    }
}