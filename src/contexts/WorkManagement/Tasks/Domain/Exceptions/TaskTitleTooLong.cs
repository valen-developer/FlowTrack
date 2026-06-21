using FlowTrack.Shared.Domain.Exception;

namespace FlowTrack.WorkManagement.Tasks.Domain;

internal sealed class TasktitleTooLong : InvalidException
{
    public TasktitleTooLong()
        : base(
            $"The task title is too long. Max is ${TaskTitle.MAX_LENGTH} characters",
            "exception.task.name.too_long"
        ) { }
}
