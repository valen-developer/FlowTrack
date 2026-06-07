using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Application;

public sealed record ActivationEmailGeneratorParams(
    Email To,
    string Token,
    string ActivationLinkBaseUrl
);
