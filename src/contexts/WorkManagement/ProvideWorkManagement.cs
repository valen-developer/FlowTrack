using FlowTrack.WorkManagement.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.WorkManagement
{
    internal static class WorkManagementServiceCollectionExtensions
    {
        public static IServiceCollection ProvideWorkManagement(this IServiceCollection services)
        {
            var dbContext = new WorkManagementDbContextFactory().CreateDbContext([]);
            dbContext.Database.Migrate();
            services.AddDbContext<WorkManagementDbContext>(options =>
                options.UseNpgsql(dbContext.Database.GetConnectionString())
            );

            return services;
        }
    }
}
