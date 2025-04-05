using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace KernelSample.Qdrant.Model;
public sealed class DataModel
{
    [VectorStoreRecordKey]
    [TextSearchResultName]
    public Guid Key { get; init; }

    [VectorStoreRecordData]
    [TextSearchResultValue]
    public string Text { get; init; }

    [VectorStoreRecordData]
    [TextSearchResultLink]
    public string Link { get; init; }

    [VectorStoreRecordData(IsFilterable = true)]
    public required string Tag { get; init; }

    [VectorStoreRecordVector(1536)]
    public ReadOnlyMemory<float> Embedding { get; init; }
}

/// <summary>
/// String mapper which converts a DataModel to a string.
/// </summary>
public sealed class DataModelTextSearchStringMapper : ITextSearchStringMapper
{
    /// <inheritdoc />
    public string MapFromResultToString(object result)
    {
        if (result is DataModel dataModel)
        {
            return dataModel.Text;
        }
        throw new ArgumentException("Invalid result type.");
    }
}

/// <summary>
/// Result mapper which converts a DataModel to a TextSearchResult.
/// </summary>
public sealed class DataModelTextSearchResultMapper : ITextSearchResultMapper
{
    /// <inheritdoc />
    public TextSearchResult MapFromResultToTextSearchResult(object result)
    {
        if (result is DataModel dataModel)
        {
            return new TextSearchResult(value: dataModel.Text) { Name = dataModel.Key.ToString(), Link = dataModel.Link };
        }
        throw new ArgumentException("Invalid result type.");
    }
}