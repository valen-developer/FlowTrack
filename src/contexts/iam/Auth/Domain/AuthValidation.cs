namespace FlowTrack.Iam.Auth.Domain;

internal sealed record AuthValidation(bool IsAuthenticated, string? UserId)
{
    public static AuthValidation Authenticated(string userId) => new(true, userId);

    public static AuthValidation Unauthenticated => new(false, null);
}
