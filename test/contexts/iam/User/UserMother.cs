using Shared;

namespace Iam.User;

public class UserMother : ObjectMother
{
    private static readonly string PasswordPattern =
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";

    public static Domain.User Random()
    {
        return new Domain.User(Guid.NewGuid(), faker.Email(), faker.FromRegex(PasswordPattern));
    }
}
