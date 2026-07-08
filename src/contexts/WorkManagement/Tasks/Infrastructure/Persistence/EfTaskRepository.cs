using FlowTrack.Shared.Domain.Dic;
using FlowTrack.Shared.Domain.FilterCriterias;
using FlowTrack.WorkManagement.Tasks.Domain;

namespace FlowTrack.WorkManagement.Tasks.Infrastructure;

[Provider(typeof(ITaskRepository), Lifetime.Singleton)]
internal sealed class EfTaskRepository : ITaskRepository
{
    public Task<Tasky?> MatchingOne(FilterCriteria criteria)
    {
        throw new NotImplementedException();
    }

    public Task Save(Tasky task)
    {
        throw new NotImplementedException();
    }
}
