using FlowTrack.Shared.Domain.ValueObjects;

namespace FlowTrack.Shared.Domain.FilterCriterias;

public record OrderBy(string PropertyName) : ValueObject<string>(PropertyName) { }
