namespace FlowTrack.Iam.Auth.Domain;

public sealed record AuthValidation(bool IsAuthenticated, string? UserId)
{
    public static AuthValidation Authenticated(string userId) => new(true, userId);

    public static AuthValidation Unauthenticated => new(false, null);
}
