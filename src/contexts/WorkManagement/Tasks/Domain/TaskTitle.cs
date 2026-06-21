using FlowTrack.Shared.Domain.ValueObjects;

namespace FlowTrack.WorkManagement.Tasks.Domain;

internal sealed record TaskTitle : ValueObject<string>
{
    public static readonly int MAX_LENGTH = 255;

    public TaskTitle(string value)
        : base(value)
    {
        EnsureTitle(value);
    }

    private static void EnsureTitle(string value)
    {
        if (value.Length > MAX_LENGTH)
        {
            throw new TasktitleTooLong();
        }
    }
}
