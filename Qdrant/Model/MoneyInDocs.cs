using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.VectorData;

namespace KernelSample.Qdrant.Model
{
    public class MoneyInDocs
    {
        [VectorStoreRecordKey]
        public ulong Id { get; set; }

        [VectorStoreRecordData(IsFilterable = true)]
        public int Index { get; set; }

        [VectorStoreRecordData]
        public string FileName { get; set; }

        [VectorStoreRecordData]
        public string Content { get; set; }

        [VectorStoreRecordVector(1536, DistanceFunction.CosineSimilarity, IndexKind.Hnsw)]
        public ReadOnlyMemory<float>? ContentEmbedding { get; set; }
    }
}