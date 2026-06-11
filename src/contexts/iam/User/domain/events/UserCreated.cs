using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Domain;

public record UserCreated(string UserId, string Email, bool IsActive) : DomainEvent, IDomainEvent
{
    public static bool External => true;

    public static string Code => "flowtrack.iam.1.event.user.created";

    public string Id { get; init; } = UserId;
    public string Email { get; set; } = Email;
    public bool IsActive { get; set; } = IsActive;

    public override bool IsExternal()
    {
        return External;
    }

    public override string GetCode()
    {
        return Code;
    }
}
