namespace FlowTrack.WorkManagement.Tasks.Domain;

internal class TaskState(TaskStateEnum state)
{
    public TaskStateEnum Value { get; } = state;
    public static readonly List<string> VALID_STATES = [.. Enum.GetNames<TaskStateEnum>()];

    public static TaskState FromString(string state)
    {
        if (Enum.TryParse<TaskStateEnum>(state, true, out var parsedState))
        {
            return new TaskState(parsedState);
        }

        throw new InvalidTaskState();
    }
}
