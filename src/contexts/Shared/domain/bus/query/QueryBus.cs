using FlowTrack.Shared.Domain;

namespace FlowTrack.Shared;

public interface IQueryBus
{
    Task<R> Ask<Q, R>(Q query)
        where Q : IQuery<R>;
}
