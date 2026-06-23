using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace FlowTrack.Shared.Infrastructure.Logging;

[Provider(typeof(IDomainLogger), Lifetime.Singleton)]
internal sealed class DomainLogger(ILogger<DomainLogger> logger) : IDomainLogger
{
    public void Info(LogMessage message) => Log(LogLevel.Information, message, null);

    public void Warning(LogMessage message) => Log(LogLevel.Warning, message, null);

    public void Error(LogMessage message, Exception? exception = null) =>
        Log(LogLevel.Error, message, exception);

    private void Log(LogLevel level, LogMessage message, Exception? exception)
    {
        using var actionScope = LogContext.PushProperty("Action", message.Action);
        using var attrScope = LogContext.PushProperty(
            "Attributes",
            message.Attributes ?? new { },
            destructureObjects: true
        );

        if (exception is null)
            logger.Log(level, message.Message);
        else
            logger.Log(level, exception, message.Message);
    }
}
