namespace FlowTrack.Iam.Users.Domain;

internal record UserCreated(string UserId, string Email, bool IsActive) : DomainEvent, IDomainEvent
{
    public static bool External => false;

    public static string Code => "flowtrack.iam.1.event.user.created";

    public string Id { get; init; } = UserId;
    public string Email { get; set; } = Email;
    public bool IsActive { get; set; } = IsActive;

    public override bool IsExternal()
    {
        return External;
    }

    public override string GetCode()
    {
        return Code;
    }
}
