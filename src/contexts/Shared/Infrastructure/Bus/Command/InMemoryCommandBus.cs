namespace FlowTrack.Shared.Infrastructure.Bus.Command
{
    [Provider(typeof(ICommandBus))]
    public sealed class InMemoryCommandBus(
        IServiceProvider serviceProvider,
        CommandHandlerInformation commandHandlerInformation
    ) : ICommandBus
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly CommandHandlerInformation _commandHandlerInformation =
            commandHandlerInformation;

        public Task Dispatch<C>(C command)
            where C : ICommand
        {
            try
            {
                var handlerType = _commandHandlerInformation.Get<C>();
                var handler = (ICommandHandler<C>)_serviceProvider.GetService(handlerType)!;
                return handler.Handle(command);
            }
            catch (Exception ex)
            {
                throw new CommandHandlerExecutionException(ex);
            }
        }
    }
}
