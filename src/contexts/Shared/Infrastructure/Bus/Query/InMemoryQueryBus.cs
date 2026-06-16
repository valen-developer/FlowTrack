using System.Diagnostics;

namespace FlowTrack.Shared.Infrastructure.Bus.Query
{
    [Provider(typeof(IQueryBus))]
    public class InMemoryQueryBus(
        IServiceProvider serviceProvider,
        QueryHandlerInformation queryHandlerInformation,
        IDomainLogger logger
    ) : IQueryBus
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly QueryHandlerInformation _queryHandlerInformation = queryHandlerInformation;
        private readonly IDomainLogger _logger = logger;

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

                _logger.Info(new LogMessage(
                    Action: "Query handled",
                    Message: $"{queryType} handled in {sw.ElapsedMilliseconds}ms",
                    Attributes: new { QueryType = queryType, ElapsedMs = sw.ElapsedMilliseconds }
                ));

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(new LogMessage(
                    Action: "Query handled",
                    Message: $"{queryType} failed",
                    Attributes: new { QueryType = queryType }
                ), ex);
                throw new QueryHandlerExecutionException(ex);
            }
        }
    }
}
