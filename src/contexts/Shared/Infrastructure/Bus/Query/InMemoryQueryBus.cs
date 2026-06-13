using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared.Infrastructure.Bus.Query;

[Provider(typeof(IQueryBus))]
public class InMemoryQueryBus(
    IServiceProvider serviceProvider,
    QueryHandlerInformation queryHandlerInformation
) : IQueryBus
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly QueryHandlerInformation _queryHandlerInformation = queryHandlerInformation;

    public async Task<R> Ask<Q, R>(Q query)
        where Q : IQuery<R>
    {
        var handlerType = _queryHandlerInformation.Get<Q, R>();
        var handler = (IQueryHandler<Q, R>)_serviceProvider.GetService(handlerType)!;

        try
        {
            return await handler.Handle(query);
        }
        catch (Exception ex)
        {
            throw new QueryHandlerExecutionException(ex);
        }
    }
}
