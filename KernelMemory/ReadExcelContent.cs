using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory.DataFormats;
using Microsoft.KernelMemory.DataFormats.Office;
using Microsoft.KernelMemory.DataFormats.Pdf;
using System;
using System.Collections.Generic;
using System.Text;

namespace KernelSample.KernelMemory;
internal class ReadExcelContent
{
    internal async Task RunAsync()
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

        string path = Path.Combine(Environment.CurrentDirectory, "Files", "MI異常排除總表0120.xlsx");

#pragma warning disable KMEXP00 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var msExcelDecoder = new MsExcelDecoder(new MsExcelDecoderConfig()
        {
        },loggerFactory);
#pragma warning restore KMEXP00 // 類型僅供評估之用，可能會在未來更新中變更或移除。抑制此診斷以繼續。
        var content = await msExcelDecoder.DecodeAsync(path);

        var sb = new StringBuilder();

        foreach (Chunk section in content.Sections)
        {
            Console.WriteLine($"Worksheet: {section.Number}/{content.Sections.Count}");
            Console.WriteLine(section.Content);
            sb.AppendLine($"Worksheet: {section.Number}/{content.Sections.Count}");
            sb.AppendLine(section.Content);
        }

        var outputPath = Path.Combine(Environment.CurrentDirectory, "Files", "MI異常排除總表0120.md");

        using (var writer = new StreamWriter(outputPath))
            await writer.WriteLineAsync(sb.ToString());

        Console.WriteLine($"xlsx content has been written to {outputPath}");
     }
}
