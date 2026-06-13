namespace FlowTrack.Shared.Domain.Bus.Command;

public interface ICommandBus
{
    Task Dispatch<C>(C command)
        where C : ICommand;
}
