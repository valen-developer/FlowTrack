namespace FlowTrack.Shared.Domain;

public sealed record JsonApiData(
    string Id,
    string Type,
    string Code,
    DateTime OcurredAt,
    object Attributes,
    object Meta
) { }
