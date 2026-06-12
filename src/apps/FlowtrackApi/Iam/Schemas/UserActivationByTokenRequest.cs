namespace FlowTrack.Iam.Schemas;

public sealed record UserActivationByTokenRequest
{
    public string Token { get; init; } = string.Empty;
}
