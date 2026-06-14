using FlowTrack.Shared.Domain.ValueObjects;

namespace FlowTrack.Shared.Domain.FilterCriterias;

public enum OrderTypes
{
    ASC,
    DESC,
    NONE,
}

public record OrderType(OrderTypes Type) : ValueObject<OrderTypes>(Type)
{
    public static OrderType Ascending => new(OrderTypes.ASC);
    public static OrderType Descending => new(OrderTypes.DESC);
    public static OrderType None => new(OrderTypes.NONE);
}
