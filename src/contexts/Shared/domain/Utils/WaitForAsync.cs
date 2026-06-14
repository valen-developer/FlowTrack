namespace FlowTrack.Shared.Domain.Utils;

public static class AsyncWaiter
{
    public static async Task WaitForAsync(
        Func<bool> predicate,
        int timeoutMs = 5000,
        int pollIntervalMs = 100,
        CancellationToken cancellationToken = default
    )
    {
        var start = DateTime.UtcNow;
        while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
        {
            if (predicate())
                return;

            await Task.Delay(pollIntervalMs, cancellationToken);
        }

        throw new TimeoutException($"Condition not met within {timeoutMs}ms");
    }

    public static async Task WaitForAsync(
        Func<Task<bool>> predicate,
        int timeoutMs = 1000,
        int pollIntervalMs = 100,
        CancellationToken cancellationToken = default
    )
    {
        var start = DateTime.UtcNow;
        while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
        {
            if (await predicate())
                return;

            await Task.Delay(pollIntervalMs, cancellationToken);
        }

        throw new TimeoutException($"Condition not met within {timeoutMs}ms");
    }
}
