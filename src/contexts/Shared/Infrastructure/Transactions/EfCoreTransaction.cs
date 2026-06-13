using FlowTrack.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FlowTrack.Shared.Infrastructure.Transactions;

public class EfCoreTransaction(DbContext dbContext) : Transaction
{
    private readonly DbContext _dbContext = dbContext;
    private IDbContextTransaction? _transaction;

    protected override async Task Initialize()
    {
        _transaction =
            _dbContext.Database.CurrentTransaction ?? _dbContext.Database.BeginTransaction();
    }

    protected override async Task Commit()
    {
        if (_transaction is null)
            return;

        await _dbContext.SaveChangesAsync();
        await _transaction.CommitAsync();
    }

    protected override async Task Release()
    {
        if (_transaction is null)
            return;

        await _transaction.DisposeAsync();
    }

    protected override async Task Rollback()
    {
        if (_transaction is null)
            return;
        await _transaction.RollbackAsync();
    }
}
