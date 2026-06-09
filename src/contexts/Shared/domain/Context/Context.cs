namespace FlowTrack.Shared.Domain;

public class Context(Transaction transaction)
{
    public Transaction Transaction { get; } = transaction;
}
