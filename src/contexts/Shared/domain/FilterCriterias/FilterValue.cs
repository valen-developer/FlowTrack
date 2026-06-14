using FlowTrack.Shared.Domain.ValueObjects;

namespace FlowTrack.Shared.Domain.FilterCriterias;

public record FilterValue(string FieldValue) : ValueObject<string>(FieldValue);
