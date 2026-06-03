using FlowTrack.Shared.Domain.Bus.Query;

namespace FlowTrack.Shared;

public interface IQueryBus
{
    Task<R> Ask<Q, R>(Q query)
        where Q : IQuery<R>;
}
