using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Auth.Application;

internal sealed record ActivationEmailGeneratorParams(
    Email To,
    string Token,
    string ActivationLinkBaseUrl
);
