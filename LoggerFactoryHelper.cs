using Microsoft.Extensions.Logging;

namespace KernelSample
{
    public class LoggerFactoryHelper
    {
        public static ILoggerFactory CreateLoggerFactory(bool enableConsoleLogging = true)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                // builder.AddFilter("Microsoft", LogLevel.Warning)
                //        .AddFilter("System", LogLevel.Warning)
                //        .AddFilter("Default", LogLevel.Debug);

                           builder.AddFilter("Microsoft", LogLevel.Warning)
                       .AddFilter("System", LogLevel.Warning)
                       .AddFilter("Default", LogLevel.Debug);

                if (enableConsoleLogging)
                {
                    builder.AddConsole();
                }
            });

            return loggerFactory;
        }
    }
}