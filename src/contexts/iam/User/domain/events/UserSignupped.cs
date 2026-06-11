using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Domain;

public record UserSignupped(string UserId, string Email) : DomainEvent, IDomainEvent
{
    public static bool External => false;

    public static string Code => "flowtrack.iam.1.event.user.signupped";

    public override string GetCode()
    {
        return Code;
    }

    public override bool IsExternal()
    {
        return External;
    }
}
