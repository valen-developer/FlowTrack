using FlowTrack.Shared.Domain;

namespace FlowTrack.WorkManagement.Tasks.Domain;

internal sealed class Tasky(
    TaskId id,
    TaskOwnerId ownerId,
    TaskTitle title,
    TaskDescription description,
    TaskState state,
    DateTime createdAt,
    DateTime updatedAt
) : AggregatedRoot
{
    public TaskId Id { get; } = id;
    public TaskOwnerId OwnerId { get; } = ownerId;
    public TaskTitle Title { get; } = title;
    public TaskDescription Description { get; } = description;
    public TaskState State { get; } = state;
    public DateTime CreatedAt { get; } = createdAt;
    public DateTime UpdatedAt { get; } = updatedAt;

    public static Tasky Create(
        TaskId Id,
        TaskOwnerId OwnerId,
        TaskTitle Title,
        TaskDescription Description,
        TaskState State
    )
    {
        var now = DateTime.UtcNow;

        var task = new Tasky(Id, OwnerId, Title, Description, State, now, now);
        var taskCreatedEvent = new TaskCreated(
            Id: Id.Value,
            OwnerId: OwnerId.Value,
            Title: Title.Value,
            Description: Description.Value,
            State: State.Value.ToString()
        );

        task.Record(taskCreatedEvent);
        return task;
    }
}
