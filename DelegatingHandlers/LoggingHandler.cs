using Microsoft.Extensions.Logging;

namespace KernelSample.DelegatingHandlers;

public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;

    public LoggingHandler(ILogger<LoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 記錄請求
        _logger.LogInformation("Request: {method} {url} {headers} {body}",
            request.Method,
            request.RequestUri,
            request.Headers,
            request.Content != null ? await request.Content.ReadAsStringAsync() : string.Empty);

        // 發送請求
        var response = await base.SendAsync(request, cancellationToken);

        // 記錄回應
        _logger.LogInformation("Response: {statusCode} {headers} {body}",
            response.StatusCode,
            response.Headers,
            response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty);

        return response;
    }
}