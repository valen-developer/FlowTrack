namespace FlowTrack.WorkManagement.Workspaces.Domain
{
    internal interface IWorkspaceSearchEngine
    {
        Task Index(Workspace workspace);
    }
}
