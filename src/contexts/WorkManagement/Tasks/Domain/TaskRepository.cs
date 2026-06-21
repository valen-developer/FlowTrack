namespace FlowTrack.WorkManagement.Tasks.Domain;

internal interface ITaskRepository
{
    abstract Task Save(Tasky task);
}
