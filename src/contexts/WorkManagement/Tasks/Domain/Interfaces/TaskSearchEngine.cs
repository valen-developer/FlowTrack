namespace FlowTrack.WorkManagement.Tasks.Domain;

internal interface ITaskSearchEngine
{
    Task Index(Tasky task);
}
