using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Domain;

public record UserCreated(Guid UserId, string Email, bool IsActive)
    : DomainEvent("iam.user.created")
{
    public override bool IsExternal()
    {
        return false;
    }
}
