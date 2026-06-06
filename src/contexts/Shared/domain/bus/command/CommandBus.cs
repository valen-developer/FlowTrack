using FlowTrack.Shared.Domain.Bus.Command;

namespace FlowTrack.Shared.Domain;

public interface ICommandBus
{
    Task Dispatch<C>(C command)
        where C : ICommand;
}
