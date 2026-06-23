using FlowTrack.Shared.Domain.FilterCriterias;

namespace FlowTrack.WorkManagement.Tasks.Domain;

internal interface ITaskRepository
{
    abstract Task Save(Tasky task);
    abstract Task<Tasky?> MatchingOne(FilterCriteria criteria);
}
