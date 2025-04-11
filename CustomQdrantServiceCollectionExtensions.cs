using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace KernelSample;

public static class CustomQdrantServiceCollectionExtensions
{
    public static IKernelBuilder AddCustomQdrantVectorStore(this IKernelBuilder builder, QdrantClient client,  QdrantVectorStoreOptions? options = default, string? serviceId = default)
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

    public static IKernelBuilder AddCustomQdrantVectorStore(this IKernelBuilder builder, 
        string grpcEndPoint,
        ILoggerFactory loggerFactory, 
        QdrantVectorStoreOptions? options = default,
        string? serviceId = default)
    {
        var channel = GrpcChannel.ForAddress(grpcEndPoint, new GrpcChannelOptions
        {
            //HttpClient = httpClient,
            //LoggerFactory = loggerFactory,
        }).Intercept(new LoggingInterceptor(loggerFactory));

        var qdrantGrpcClient = new QdrantGrpcClient(channel);
        var qdrantClient = new QdrantClient(qdrantGrpcClient);

        builder.Services.AddKeyedTransient<IVectorStore>(
            serviceId,
            (sp, obj) =>
            {
                var selectedOptions = options ?? sp.GetService<QdrantVectorStoreOptions>();

                return new QdrantVectorStore(
                    qdrantClient,
                    selectedOptions);
            });

        return builder;
    }
}
