using FlowTrack.Shared.Domain.ValueObjects;

namespace FlowTrack.Shared.Domain.FilterCriterias;

public record FilterOperator(FilterOperators Type) : ValueObject<FilterOperators>(Type);
