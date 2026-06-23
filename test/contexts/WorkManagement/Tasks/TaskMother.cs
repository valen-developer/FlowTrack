using FlowTrack.Shared.Test;
using FlowTrack.WorkManagement.Tasks.Domain;

namespace FlowTrack.WorkManagement.Tasks.Test;

internal class TaskMother : ObjectMother
{
    internal static TaskId TaskId => new(Faker.uuid());
    internal static TaskOwnerId TaskOwnerId => new(Faker.uuid());
    internal static TaskWorkspaceId TaskWorkspaceId => new(Faker.uuid());
    internal static TaskTitle TaskTitle => new(faker.Words(3));
    internal static TaskDescription TaskDescription => new(faker.Words(10));
    internal static TaskState TaskState => new(TaskStateEnum.TODO);

    internal static Tasky WithId(string id)
    {
        return new Tasky(
            id: new TaskId(id),
            ownerId: TaskOwnerId,
            workspaceId: TaskWorkspaceId,
            title: TaskTitle,
            description: TaskDescription,
            state: TaskState,
            createdAt: DateTime.UtcNow,
            updatedAt: DateTime.UtcNow
        );
    }
}
