namespace FlowTrack.Shared.Domain.Transactions;

public abstract class Transaction
{
    private readonly List<Func<Task>> _onFailedCallbacks = new();
    private readonly List<Func<Task>> _runOutsideOfTransactionCallbacks = new();

    public void OnFailed(Func<Task> callback)
    {
        _onFailedCallbacks.Add(callback);
    }

    public void RunOutsideOfTransaction(Func<Task> callback)
    {
        _runOutsideOfTransactionCallbacks.Add(callback);
    }

    public async Task<T> RunInTransaction<T>(Func<Task<T>> action)
    {
        try
        {
            await Initialize();
            var result = await action();
            await Commit();
            return result;
        }
        catch
        {
            await Rollback();
            await ExecuteOnFailedCallbacks();
            throw;
        }
        finally
        {
            await ExecuteRunOutsideOfTransactionCallbacks();
            await Release();
        }
    }

    private async Task ExecuteOnFailedCallbacks()
    {
        foreach (var callback in _onFailedCallbacks)
        {
            await callback();
        }
    }

    private async Task ExecuteRunOutsideOfTransactionCallbacks()
    {
        foreach (var callback in _runOutsideOfTransactionCallbacks)
        {
            await callback();
        }
    }

    protected abstract Task Initialize();
    protected abstract Task Commit();
    protected abstract Task Rollback();
    protected abstract Task Release();
}
