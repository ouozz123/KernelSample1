using Google.Protobuf;
using System.IO.Compression;
using System.Text;

namespace KernelSample;
public class HttpLogger : DelegatingHandler
{
    public static HttpClient GetHttpClient(bool log = false)
    {
        return log
            ? new HttpClient(new HttpLogger(new HttpClientHandler()))
            : new HttpClient();
    }

    public HttpLogger(HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 設置 Accept-Charset 標頭為 UTF-8
        request.Headers.AcceptCharset.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("utf-8"));


        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Request:");
        Console.WriteLine(request.ToString());
        if (request.Content != null)
        {
            Console.WriteLine(await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        }

        Console.WriteLine();

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Response:");
        Console.WriteLine(response.ToString());
        if (response.Content != null)
        {
            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

            var contextType = response.Headers.Select(x => x.Key == "application/grpc").FirstOrDefault();

            //if (contextType != null)
            //    content = DecompressGrpcResponse(content, "gzip"); // or "identity", "deflate"

            // 檢查伺服器返回的編碼
            var charset = response.Content.Headers.ContentType?.CharSet ?? "utf-8";

            var encoding = Encoding.GetEncoding(charset);
            var contentString = encoding.GetString(content);
            Console.WriteLine(contentString);
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        return response;
    }

    public static byte[] DecompressGrpcResponse(byte[] data, string encoding)
    {
        if (encoding == "gzip")
        {
            using var compressedStream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            compressedStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
        else if (encoding == "deflate")
        {
            using var compressedStream = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            compressedStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
        else // identity or unknown
        {
            return data;
        }
    }
}
