using FlowTrack.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FlowTrack.Iam.Shared.Infrastructure;

public class IamDbContextFactory : IDesignTimeDbContextFactory<IamDbContext>
{
    public IamDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                IamEnvironmentKeysEnum.IAM_DB_CONNECTION_STRING.ToString()
            )
            ?? throw new InvalidOperationException(
                $"Environment variable '{IamEnvironmentKeysEnum.IAM_DB_CONNECTION_STRING}' is not set."
            );

        var optionsBuilder = new DbContextOptionsBuilder<IamDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new IamDbContext(optionsBuilder.Options);
    }
}
