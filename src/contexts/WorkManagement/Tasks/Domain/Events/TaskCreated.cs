using FlowTrack.Shared.Domain.Bus.Event;

namespace FlowTrack.WorkManagement.Tasks.Domain;

internal sealed record TaskCreated(
    string Id,
    string OwnerId,
    string Title,
    string Description,
    string State
) : DomainEvent, IDomainEvent
{
    public static bool External => false;

    public static string Code => "flowtrack.workmanagement.1.event.task.created";

    public override string GetCode()
    {
        return Code;
    }

    public override bool IsExternal()
    {
        return External;
    }
}
