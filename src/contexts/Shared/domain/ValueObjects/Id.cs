using FlowTrack.Shared.Domain.Exception;
using FlowTrack.Shared.Domain.ValueObjects;

namespace FlowTrack.Shared.Domain;

public record Uuid : ValueObject<string>
{
    public Uuid(string value, DomainException invalidException)
        : base(value)
    {
        EnsureUUID(value, invalidException);
    }

    private static void EnsureUUID(string value, DomainException invalidException)
    {
        if (!Guid.TryParse(value, out _))
        {
            throw invalidException;
        }
    }
}
