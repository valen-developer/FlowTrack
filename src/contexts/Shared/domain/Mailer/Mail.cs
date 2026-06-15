namespace FlowTrack.Shared.Domain.Mailer
{
    public class Mail(string to, string subject, string body)
    {
        public string To { get; init; } = to;
        public string Subject { get; init; } = subject;
        public string Body { get; init; } = body;
    }
}
