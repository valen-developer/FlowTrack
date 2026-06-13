using System.Text.RegularExpressions;

namespace FlowTrack.Iam.Test.Users;

public class UserMother : ObjectMother
{
    private static readonly Regex PasswordPattern = InvalidPassword.Regex;

    public static User Random()
    {
        return new User(Id(), Email(), Password(), faker.boolean());
    }

    public static User Active()
    {
        return new User(Id(), Email(), Password(), true);
    }

    public static User Inactive()
    {
        return new User(Id(), Email(), Password(), false);
    }

    private static UserId Id()
    {
        return new UserId(Guid.NewGuid().ToString());
    }

    private static UserEmail Email()
    {
        return new UserEmail(faker.Email());
    }

    private static UserPassword Password()
    {
        return new UserPassword(faker.FromRegex(PasswordPattern));
    }
}
