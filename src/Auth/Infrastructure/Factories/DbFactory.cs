using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Factories
{
    public static class DbFactory
    {
        public static IServiceCollection AddDbProvider(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var dbProvider = configuration.GetValue<string>("DatabaseProvider");
            Console.WriteLine($"Using database provider: {dbProvider}");

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("DbFactory");

                if (dbProvider != null && dbProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
                {
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
                    }
                    
                    logger.LogInformation("Connecting to PostgreSQL database."); // Removed connection string from log
                    options.UseNpgsql(connectionString);
                }
                else
                {
                    logger.LogInformation("Using in-memory database.");
                    options.UseInMemoryDatabase("AuthDb");
                }
            });

            return services;
        }
    }
}
