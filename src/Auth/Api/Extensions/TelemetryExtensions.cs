
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Auth.Api.Extensions
{
    public static class TelemetryExtensions
    {
        public static IServiceCollection AddTelemetryServices(this IServiceCollection services)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("auth-service"))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddConsoleExporter());

            return services;
        }
    }
}
