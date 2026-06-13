namespace FlowTrack.Shared.Domain.ValueObjects;

public record ValueObject<T>
{
    public T Value { get; }

    public ValueObject(T value)
    {
        Value = value;
    }
}
