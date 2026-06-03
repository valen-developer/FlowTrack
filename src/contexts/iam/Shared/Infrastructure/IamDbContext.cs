using Microsoft.EntityFrameworkCore;

namespace FlowTrack.Iam.Infrastructure;

public class IamDbContext(DbContextOptions<IamDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IamDbContext).Assembly);
    }
}
