namespace FlowTrack.Shared;

public class QueryHandlerExecutionException(Exception cause)
    : ActionHandlerExecutionException(cause) { }
