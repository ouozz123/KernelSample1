using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

namespace KernelSample;

public static class CustomQdrantServiceCollectionExtensions
{
    public static IKernelBuilder AddCustomQdrantVectorStore(this IKernelBuilder builder, QdrantClient client, QdrantVectorStoreOptions? options = default, string? serviceId = default)
    {
        builder.Services.AddKeyedTransient<IVectorStore>(
            serviceId,
            (sp, obj) =>
            {
                var selectedOptions = options ?? sp.GetService<QdrantVectorStoreOptions>();

                return new QdrantVectorStore(
                    client,
                    selectedOptions);
            });

        return builder;
    }
}
