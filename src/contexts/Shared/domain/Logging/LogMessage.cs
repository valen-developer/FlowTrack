namespace FlowTrack.Shared.Domain.Logging;

public sealed record LogMessage(string Action, string Message, object? Attributes = null);
