using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FlowTrack.Shared.Infrastructure.Bus.Command
{
    [Provider(typeof(ICommandBus))]
    public sealed class InMemoryCommandBus(
        IServiceProvider serviceProvider,
        CommandHandlerInformation commandHandlerInformation,
        ILogger<InMemoryCommandBus> logger
    ) : ICommandBus
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly CommandHandlerInformation _commandHandlerInformation =
            commandHandlerInformation;
        private readonly ILogger<InMemoryCommandBus> _logger = logger;

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

                _logger.LogInformation(
                    "Command {CommandType} handled in {ElapsedMs}ms",
                    commandType,
                    sw.ElapsedMilliseconds
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Command {CommandType} failed",
                    commandType
                );
                throw new CommandHandlerExecutionException(ex);
            }
        }
    }
}
