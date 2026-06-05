using System.Text.RegularExpressions;
using FlowTrack.Iam.Domain;
using FlowTrack.Shared.Test;

namespace FlowTrack.Iam.Test;

public class UserMother : ObjectMother
{
    private static readonly Regex PasswordPattern = InvalidPassword.Regex;

    public static User Random()
    {
        return new User(
            Guid.NewGuid(),
            faker.Email(),
            faker.FromRegex(PasswordPattern),
            faker.boolean()
        );
    }

    public static User Active()
    {
        return new User(Guid.NewGuid(), faker.Email(), faker.FromRegex(PasswordPattern), true);
    }

    public static User Inactive()
    {
        return new User(
            Guid.NewGuid(),
            faker.Email(),
            faker.FromRegex(InvalidPassword.Regex),
            false
        );
    }
}
