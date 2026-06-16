using FlowTrack.WorkManagement.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.WorkManagement
{
    internal static class WorkManagementServiceCollectionExtensions
    {
        public static IServiceCollection ProvideWorkManagement(this IServiceCollection services)
        {
            string connectionString;
            using (var dbContext = new WorkManagementDbContextFactory().CreateDbContext([]))
            {
                dbContext.Database.Migrate();
                connectionString =
                    dbContext.Database.GetConnectionString()
                    ?? throw new InvalidOperationException(
                        "WORK_MANAGEMENT_DB_CONNECTION_STRING is not set or could not be resolved."
                    );
            }

            services.AddDbContext<WorkManagementDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

            return services;
        }
    }
}
