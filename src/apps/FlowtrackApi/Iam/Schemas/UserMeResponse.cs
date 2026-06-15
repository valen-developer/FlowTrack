namespace FlowtrackApi.Iam.Schemas
{
    public sealed record UserMeResponse(string Id, string Email)
    {
        internal static UserMeResponse FromUser(User user) => new(user.Id.Value, user.Email.Value);
    }
}
