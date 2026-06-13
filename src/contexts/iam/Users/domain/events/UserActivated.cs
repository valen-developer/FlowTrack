using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Users.Domain;

internal sealed record UserActivated(string Id) : DomainEvent, IDomainEvent
{
    public string Id { get; init; } = Id;

    public static bool External => false;

    public static string Code => "flowtrack.iam.1.event.user.activated";

    public override string GetCode()
    {
        return Code;
    }

    public override bool IsExternal()
    {
        return External;
    }
}
