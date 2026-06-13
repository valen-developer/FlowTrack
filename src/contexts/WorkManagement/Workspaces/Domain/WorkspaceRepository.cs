namespace FlowTrack.WorkManagement.Workspaces.Domain;

internal interface IWorkspaceRepository
{
    public abstract Task Save(Workspace workspace);
}
