using FlowTrack.Shared.Domain.FilterCriterias;

namespace FlowTrack.WorkManagement.Workspaces.Domain;

internal interface IWorkspaceSearchEngine
{
    Task Index(Workspace workspace);
    Task<List<Workspace>> Find(FilterCriteria criteria);
}
