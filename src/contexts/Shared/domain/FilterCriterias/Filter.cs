namespace FlowTrack.Shared.Domain.FilterCriterias
{
    public record Filter(FilterField Field, FilterOperator Operator, FilterValue Value);
}
