using FlowTrack.WorkManagement.Workspaces.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowTrack.WorkManagement.Shared.Infrastructure;

internal class WorkManagementDbContext(DbContextOptions<WorkManagementDbContext> options)
    : DbContext(options)
{
    public DbSet<WorkspaceEntity> Workspaces => Set<WorkspaceEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkManagementDbContext).Assembly);
    }
}
