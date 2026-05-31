using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Test;

namespace FlowTrack.Iam.Test;

public class UserMother : ObjectMother
{
    private static readonly string PasswordPattern =
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";

    public static User Random()
    {
        return new User(Guid.NewGuid(), faker.Email(), faker.FromRegex(PasswordPattern));
    }
}
