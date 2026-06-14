namespace FlowTrack.Shared.Domain.FilterCriterias;

public record Order(OrderBy OrderBy, OrderType OrderType)
{
    public static Order None => new(new OrderBy(string.Empty), OrderType.None);
    public static Order Asc => new(new OrderBy(string.Empty), OrderType.Ascending);
    public static Order Desc => new(new OrderBy(string.Empty), OrderType.Descending);
}
