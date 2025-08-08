using Microsoft.Extensions.Logging;

namespace Grad.CsLib.Errors;

static partial class LoggingExtensions
{
    [LoggerMessage(3, LogLevel.Error, "[{@exceptionType}] at [{@route}] due to [{@reason}]")]
    public static partial void LogStructuredException(this ILogger l, Exception ex, string? exceptionType, string? route, string? reason);

    [LoggerMessage(4, LogLevel.Error, """
                                      =================================
                                      {route}
                                      TYPE: {exceptionType}
                                      REASON: {reason}
                                      ---------------------------------
                                      {stackTrace}
                                      """)]
    public static partial void LogUnStructuredException(this ILogger l, string? exceptionType, string? route, string? reason, string? stackTrace);
}