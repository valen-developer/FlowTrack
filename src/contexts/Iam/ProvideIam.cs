using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FlowTrack.Iam
{
    internal static class IamServiceCollectionExtensions
    {
        public static IServiceCollection ProvideIam(this IServiceCollection services)
        {
            string connectionString;
            using (var iamDbContext = new IamDbContextFactory().CreateDbContext([]))
            {
                iamDbContext.Database.Migrate();
                connectionString =
                    iamDbContext.Database.GetConnectionString()
                    ?? throw new InvalidOperationException(
                        "IAM_DB_CONNECTION_STRING is not set or could not be resolved."
                    );
            }

            services.AddDbContext<IamDbContext>(options => options.UseNpgsql(connectionString));

            return services;
        }
    }
}
