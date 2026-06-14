using FlowTrack.WorkManagement.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FlowTrack.WorkManagement.Shared.Infrastructure;

internal class WorkManagementDbContextFactory : IDesignTimeDbContextFactory<WorkManagementDbContext>
{
    public WorkManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                WorkManagementEnvironmentKeysEnum.WORK_MANAGEMENT_DB_CONNECTION_STRING.ToString()
            )
            ?? throw new InvalidOperationException(
                $"Environment variable '{WorkManagementEnvironmentKeysEnum.WORK_MANAGEMENT_DB_CONNECTION_STRING}' is not set."
            );

        var optionsBuilder = new DbContextOptionsBuilder<WorkManagementDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new WorkManagementDbContext(optionsBuilder.Options);
    }
}
