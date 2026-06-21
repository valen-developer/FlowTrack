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
    public TaskId Id { get; } = id;
    public TaskTitle Title { get; } = title;
    public TaskDescription Description { get; } = description;
    public TaskState State { get; } = state;
    public DateTime CreatedAt { get; } = createdAt;
    public DateTime UpdatedAt { get; } = updatedAt;

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
