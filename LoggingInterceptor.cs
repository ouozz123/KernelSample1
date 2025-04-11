using DnsClient.Internal;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;

namespace KernelSample;

public class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(Microsoft.Extensions.Logging.ILoggerFactory factory)
    {
        _logger = factory.CreateLogger<LoggingInterceptor>();
    }


    /// <summary>
    /// 伺服器攔截方式
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="continuation"></param>
    /// <returns></returns>
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation("Request: {method} {request}", context.Method, request);

        Console.WriteLine("123");

        var response = await continuation(request, context);

        _logger.LogInformation("Response: {method} {response}", context.Method, response);

        return response;
    }


    /// <summary>
    /// Client 攔截方式
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="continuation"></param>
    /// <returns></returns>
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
          TRequest request,
          ClientInterceptorContext<TRequest, TResponse> context,
          AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        //幫我補上 AsyncUnaryCall 的 log 需紀錄 request 跟 response
        var call = continuation(request, context);

        var methodFullName = context.Method.FullName;
        var baseUri = context.Options.Headers?.GetValue("BaseAddress") ?? "http://localhost:6334";
        var fullEndpoint = $"{baseUri}{methodFullName}";
        _logger.LogInformation("Request: {FullEndpoint} {@Request}", fullEndpoint, request);

        return new AsyncUnaryCall<TResponse>(
            HandleResponse(call.ResponseAsync),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    /// <summary>
    /// Handles the response of an asynchronous task and manages exceptions.
    /// </summary>
    /// <typeparam name="TResponse">This type parameter represents the expected result type of the asynchronous operation.</typeparam>
    /// <param name="inner">This parameter is the asynchronous task whose result is being awaited.</param>
    /// <returns>Returns the result of the awaited task.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an exception occurs during the execution of the awaited task.</exception>
    private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> inner)
    {
        try
        {
            var response = await inner;
            _logger.LogInformation("Response: {@Response}", response);
            return response;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Custom error", ex);
        }
    }
}