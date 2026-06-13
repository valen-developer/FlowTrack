namespace FlowTrack.Shared.Domain.Bus.Command;

public class CommandHandlerExecutionException(System.Exception cause)
    : ActionHandlerExecutionException(cause) { }
