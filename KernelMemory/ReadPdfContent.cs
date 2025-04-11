using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory.DataFormats;
using Microsoft.KernelMemory.DataFormats.Office;
using Microsoft.KernelMemory.DataFormats.Pdf;
using System;
using System.Collections.Generic;
using System.Text;

namespace KernelSample.KernelMemory;
internal class ReadPdfContent : Sample
{
    internal override async Task RunAsync(string apiKey)
    {
        #region 建立 logger
        Console.OutputEncoding = Encoding.UTF8;

        // 建立 LoggerFactory 實例
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole((x) =>
                {
                })
                .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug); // 設置最低日誌級別
        });

        #endregion

#pragma warning disable KMEXP00 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var pdfDecoder = new PdfDecoder(loggerFactory);
#pragma warning restore KMEXP00 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。

        string miPdfPath = Path.Combine(Environment.CurrentDirectory, "Files", "錢進系統MI操作手冊20241211.pdf");

        //CancellationToken 實作
        //CancellationTokenSource cts = new CancellationTokenSource();
        //CancellationToken token = cts.Token;
        //cts.CancelAfter(TimeSpan.FromSeconds(5)); // 5秒後取消
        //cts.Cancel(); // 立即取消
        //cts.Dispose(); // 釋放資源

        var content = await pdfDecoder.DecodeAsync(miPdfPath);
        var sb = new StringBuilder();
        var i = 1;
        foreach (Chunk section in content.Sections)
        {
            sb.AppendLine($"Page: {section.Number}/{content.Sections.Count}");

            if (i == 1)
            {
                sb.AppendLine(section.Content);
                sb.AppendLine("--------");
                i++;
                continue;
            }

            //將 Sections 依照換行符號分割，去除前四個後再組成字串
            var lines = section.Content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var filteredLines = lines.Skip(4).ToArray();
            var result = string.Join(Environment.NewLine, filteredLines);
            sb.AppendLine(result);
            sb.AppendLine("--------");

            i++;
        }

        Console.WriteLine(sb.ToString());

        //寫入結果至 md 檔案
        var outputPath = Path.Combine(Environment.CurrentDirectory, "Files", "錢進系統MI操作手冊20241211.md");

        using (var writer = new StreamWriter(outputPath))
            await writer.WriteLineAsync(sb.ToString());

        Console.WriteLine($"PDF content has been written to {outputPath}");
    }
}
