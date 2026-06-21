using FlowTrack.Shared.Domain;

namespace FlowTrack.WorkManagement.Tasks.Domain;

internal sealed class Tasky(
    TaskId id,
    TaskTitle title,
    TaskDescription description,
    TaskState state,
    DateTime createdAt,
    DateTime updatedAt
) : AggregatedRoot
{
    private TaskId _id = id;
    private TaskTitle _title = title;
    private TaskDescription _description = description;
    private TaskState _state = state;
    private DateTime _createdAt = createdAt;
    private DateTime _updatedAt = updatedAt;

    public static Tasky Create(
        TaskId Id,
        TaskTitle Title,
        TaskDescription Description,
        TaskState State
    )
    {
        var now = DateTime.UtcNow;
        return new Tasky(Id, Title, Description, State, now, now);
    }
}
