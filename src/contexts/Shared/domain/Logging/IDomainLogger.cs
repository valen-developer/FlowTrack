namespace FlowTrack.Shared.Domain.Logging;

public interface IDomainLogger
{
    void Info(LogMessage message);
    void Warning(LogMessage message);
    void Error(LogMessage message, System.Exception? exception = null);
}
