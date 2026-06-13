namespace FlowtrackApi.Iam.Schemas;

public sealed record UserMeResponse(string Id, string Email)
{
    public static UserMeResponse FromUser(User user) => new(user.Id.Value, user.Email.Value);
}
