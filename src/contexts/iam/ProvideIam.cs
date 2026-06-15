using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.Iam
{
    internal static class IamServiceCollectionExtensions
    {
        public static IServiceCollection ProvideIam(this IServiceCollection services)
        {
            var iamDbContext = new IamDbContextFactory().CreateDbContext([]);
            iamDbContext.Database.Migrate();
            services.AddDbContext<IamDbContext>(options =>
                options.UseNpgsql(iamDbContext.Database.GetConnectionString())
            );

            return services;
        }
    }
}
