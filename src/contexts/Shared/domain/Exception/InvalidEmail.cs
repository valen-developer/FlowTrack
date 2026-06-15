namespace FlowTrack.Shared.Domain.Exception
{
    public class InvalidEmail : InvalidException
    {
        public InvalidEmail()
            : base("Invalid email", "exception.email.invalid") { }

        public InvalidEmail(string email)
            : base($"Invalid email: {email}", "exception.email.invalid") { }
    }
}
