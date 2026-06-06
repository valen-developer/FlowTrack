namespace FlowTrack.Shared.Domain;

public interface IQueryHandler<Q, R>
    where Q : IQuery<R>
{
    Task<R> Handle(Q query);
}
