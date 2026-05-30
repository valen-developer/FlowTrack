namespace Shared.Domain.Bus.Query;



public interface IQueryHandler<Q, R> where Q : IQuery<R>
{

  Task<R> Handle(Q query);

}