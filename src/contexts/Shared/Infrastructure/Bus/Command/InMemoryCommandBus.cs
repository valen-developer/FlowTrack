using FlowTrack.Shared.Domain;
using FlowTrack.Shared.Domain.Bus.Command;

namespace FlowTrack.Shared.Infrastructure.Bus.Command;

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
        var handlerType = _commandHandlerInformation.Get<C>();
        var handler = (ICommandHandler<C>)_serviceProvider.GetService(handlerType)!;

        try
        {
            return handler.Handle(command);
        }
        catch (Exception ex)
        {
            throw new CommandHandlerExecutionException(ex);
        }
    }
}
