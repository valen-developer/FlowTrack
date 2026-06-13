namespace FlowTrack.Shared.Domain.Contexts;

public class Context(Transaction transaction)
{
    public Transaction Transaction { get; } = transaction;
}
