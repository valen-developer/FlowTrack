namespace FlowTrack.Shared.Domain.FilterCriterias;

public record FilterCriteria(Filters Filters, Order Order, int? Limit = null, int? Offset = null);
