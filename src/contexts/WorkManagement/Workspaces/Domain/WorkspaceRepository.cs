using FlowTrack.Shared.Domain.FilterCriterias;

namespace FlowTrack.WorkManagement.Workspaces.Domain;

internal interface IWorkspaceRepository
{
    public Task<List<Workspace>> Matching(FilterCriteria criteria);
    public abstract Task Save(Workspace workspace);
}
