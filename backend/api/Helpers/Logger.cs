using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace api.Helpers;

public static class Logger
{
    public static void CustomDebug(this ILogger logger, string message)
    {
        Log.Logger.ForContext("CDebug", true).Debug(message);
    }
}
