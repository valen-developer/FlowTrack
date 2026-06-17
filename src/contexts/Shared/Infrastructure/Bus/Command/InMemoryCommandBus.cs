using System.Diagnostics;
using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.Shared.Infrastructure.Bus.Command
{
    [Provider(typeof(ICommandBus))]
    public sealed class InMemoryCommandBus(
        IServiceProvider serviceProvider,
        CommandHandlerInformation commandHandlerInformation,
        IDomainLogger logger
    ) : ICommandBus
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly CommandHandlerInformation _commandHandlerInformation =
            commandHandlerInformation;
        private readonly IDomainLogger _logger = logger;

        public async Task Dispatch<C>(C command)
            where C : ICommand
        {
            var commandType = typeof(C).Name;

            try
            {
                var sw = Stopwatch.StartNew();
                var handlerType = _commandHandlerInformation.Get<C>();
                var handler = (ICommandHandler<C>)_serviceProvider.GetService(handlerType)!;
                await handler.Handle(command);
                sw.Stop();

                _logger.Info(
                    new LogMessage(
                        Action: "Command handled",
                        Message: $"{commandType} handled in {sw.ElapsedMilliseconds}ms",
                        Attributes: new
                        {
                            CommandType = commandType,
                            ElapsedMs = sw.ElapsedMilliseconds,
                        }
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.Error(
                    new LogMessage(
                        Action: "Command handled",
                        Message: $"{commandType} failed",
                        Attributes: new Dictionary<string, object> { [commandType] = command }
                    ),
                    ex
                );

                if (ex is DomainException)
                {
                    throw;
                }

                throw new CommandHandlerExecutionException(ex);
            }
        }
    }
}
