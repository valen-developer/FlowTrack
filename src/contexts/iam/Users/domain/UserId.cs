namespace FlowTrack.Iam.Users.Domain;

public class UserId
{
    public string Value { get; }

    public UserId(string value)
    {
        EnsureUUID(value);
        Value = value;
    }

    private static void EnsureUUID(string value)
    {
        if (!Guid.TryParse(value, out _))
        {
            throw new InvalidUserId();
        }
    }
}
