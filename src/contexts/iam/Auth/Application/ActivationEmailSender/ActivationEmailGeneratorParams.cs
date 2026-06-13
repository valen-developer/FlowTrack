using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Auth.Application;

public sealed record ActivationEmailGeneratorParams(
    Email To,
    string Token,
    string ActivationLinkBaseUrl
);
