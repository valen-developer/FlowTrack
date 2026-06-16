namespace FlowTrack.Shared.Infrastructure.Logging;

public static class CorrelationContext
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    public static string? Get() => _correlationId.Value;

    public static void Set(string? correlationId) => _correlationId.Value = correlationId;

    public static string Generate() => Guid.NewGuid().ToString("N");
}
