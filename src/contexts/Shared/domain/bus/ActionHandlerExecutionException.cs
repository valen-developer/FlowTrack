namespace FlowTrack.Shared;

public abstract class ActionHandlerExecutionException(Exception cause) : Exception
{
    public Exception GetCause() => cause;
}
