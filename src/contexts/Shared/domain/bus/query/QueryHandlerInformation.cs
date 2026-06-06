namespace FlowTrack.Shared.Domain;

public sealed class QueryHandlerInformation
{
    private readonly Dictionary<Type, Type> _handlers = new();

    public void Add(Type queryType, Type handlerType)
    {
        if (!typeof(IQuery).IsAssignableFrom(queryType))
            throw new ArgumentException(
                $"Type '{queryType.FullName}' does not implement IQuery.",
                nameof(queryType)
            );

        var queryHandlerInterface =
            handlerType
                .GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
                )
            ?? throw new ArgumentException(
                $"Type '{handlerType.FullName}' does not implement IQueryHandler<,>.",
                nameof(handlerType)
            );
        var handledQueryType = queryHandlerInterface.GetGenericArguments()[0];

        if (handledQueryType != queryType)
            throw new ArgumentException(
                $"Handler '{handlerType.FullName}' handles query '{handledQueryType.FullName}', not '{queryType.FullName}'.",
                nameof(handlerType)
            );

        if (_handlers.ContainsKey(queryType))
            throw new InvalidOperationException($"Query '{queryType.Name}' is already registered.");

        _handlers[queryType] = handlerType;
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
