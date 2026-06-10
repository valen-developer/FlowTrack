using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Domain;

public record UserCreated(Guid UserId, string Email, bool IsActive)
    : DomainEvent("iam.user.created")
{
    public Guid Id { get; init; } = UserId;
    public string Email { get; set; } = Email;
    public bool IsActive { get; set; } = IsActive;

    public override bool IsExternal()
    {
        return true;
    }
}
