namespace FlowTrack.Shared.Domain.Bus;

public abstract class ActionHandlerExecutionException(System.Exception cause) : System.Exception
{
    public System.Exception GetCause() => cause;
}
