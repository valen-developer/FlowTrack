namespace FlowTrack.Shared.Domain;

public class CommandHandlerExecutionException(Exception cause)
    : ActionHandlerExecutionException(cause) { }
