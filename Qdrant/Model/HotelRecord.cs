namespace KernelSample.Qdrant.Model;

internal class HotelRecord
{
    /// <summary>
    /// 日期 (格式為 yyyy-MM-dd)
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 類別 (例如：資訊、活動等)
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// 標題
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 點閱數
    /// </summary>
    public int Views { get; set; }

    /// <summary>
    /// 聯絡人 (例如：email 或其他聯絡資訊)
    /// </summary>
    public string Contact { get; set; }

    /// <summary>
    /// 網址 (URL)
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 備註
    /// </summary>
    public string Notes { get; set; }
}