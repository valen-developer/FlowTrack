namespace FlowTrack.Shared.Domain.Bus.Query;

public sealed class QueryHandlerInformation
{
    private readonly Dictionary<Type, Type> _handlers = new();

    public void Add<Q, H, R>()
        where Q : IQuery<R>
        where H : IQueryHandler<Q, R>
    {
        var queryType = typeof(Q);

        if (_handlers.ContainsKey(queryType))
            throw new InvalidOperationException($"Query '{queryType.Name}' is already registered.");

        _handlers[queryType] = typeof(H);
    }

    public Type Get<Q, R>()
        where Q : IQuery
    {
        var queryType = typeof(Q);

        if (_handlers.TryGetValue(queryType, out var handlerType))
            return handlerType;

        throw new InvalidOperationException($"Query '{queryType.Name}' is not registered.");
    }
}
