using FlowTrack.Iam.Users.Domain;

namespace FlowTrackIamApi.Schemas;

public sealed record UserMeResponse(string Id, string Email)
{
    internal static UserMeResponse FromUser(User user) => new(user.Id.Value, user.Email.Value);
}
