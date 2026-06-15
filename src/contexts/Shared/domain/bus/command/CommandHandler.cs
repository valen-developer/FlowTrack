namespace FlowTrack.Shared.Domain.Bus.Command
{
    public interface ICommandHandler<C>
        where C : ICommand
    {
        Task Handle(C command);
    }
}
