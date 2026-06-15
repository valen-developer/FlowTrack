namespace FlowTrack.Shared.Domain.JsonApiSchemas
{
    public sealed record JsonApiData(
        string Id,
        string Type,
        string Code,
        DateTime OcurredAt,
        object Attributes,
        object Meta
    ) { }
}
