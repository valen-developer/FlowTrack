namespace FlowTrack.Shared.Domain.Bus.Query;

public interface IQueryBus
{
    Task<R> Ask<Q, R>(Q query)
        where Q : IQuery<R>;
}
