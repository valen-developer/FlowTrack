namespace FlowTrack.Shared.Domain.Bus.Query;

public class QueryHandlerExecutionException(System.Exception cause)
    : ActionHandlerExecutionException(cause) { }
