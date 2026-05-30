using FlowTrack.Shared.Test;
using DomainUser = FlowTrack.Iam.User.Domain.User;

namespace FlowTrack.Iam.Test.User;

public class UserMother : ObjectMother
{
    private static readonly string PasswordPattern =
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";

    public static DomainUser Random()
    {
        return new DomainUser(Guid.NewGuid(), faker.Email(), faker.FromRegex(PasswordPattern));
    }
}
