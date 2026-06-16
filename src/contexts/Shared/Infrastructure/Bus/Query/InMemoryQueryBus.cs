using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FlowTrack.Shared.Infrastructure.Bus.Query
{
    [Provider(typeof(IQueryBus))]
    public class InMemoryQueryBus(
        IServiceProvider serviceProvider,
        QueryHandlerInformation queryHandlerInformation,
        ILogger<InMemoryQueryBus> logger
    ) : IQueryBus
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly QueryHandlerInformation _queryHandlerInformation = queryHandlerInformation;
        private readonly ILogger<InMemoryQueryBus> _logger = logger;

        public async Task<R> Ask<Q, R>(Q query)
            where Q : IQuery<R>
        {
            var queryType = typeof(Q).Name;

            try
            {
                var handlerType = _queryHandlerInformation.Get<Q, R>();
                var handler = (IQueryHandler<Q, R>)_serviceProvider.GetService(handlerType)!;

                var sw = Stopwatch.StartNew();
                var result = await handler.Handle(query);
                sw.Stop();

                _logger.LogInformation(
                    "Query {QueryType} handled in {ElapsedMs}ms",
                    queryType,
                    sw.ElapsedMilliseconds
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Query {QueryType} failed", queryType);
                throw new QueryHandlerExecutionException(ex);
            }
        }
    }
}
