using Microsoft.Extensions.Logging;

namespace nsWinLogger
{
    class cWinLogger
    {
        public static ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddEventLog(eventLogSettings => eventLogSettings.SourceName = "Anemorumbometer"));
        public static ILogger Logger = factory.CreateLogger("AnemoLog");
    }
}
