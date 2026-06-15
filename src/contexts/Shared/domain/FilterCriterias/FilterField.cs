using FlowTrack.Shared.Domain.ValueObjects;

namespace FlowTrack.Shared.Domain.FilterCriterias
{
    public record FilterField(string FieldName) : ValueObject<string>(FieldName);
}
