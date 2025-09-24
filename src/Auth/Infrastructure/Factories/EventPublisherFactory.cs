using Auth.Application.Interfaces;
using Auth.Infrastructure.EventBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client.Core;

namespace Auth.Infrastructure.Factories
{
    public static class EventPublisherFactory
    {
        public static void AddEventPublisher(IServiceCollection services, IConfiguration configuration)
        {
            var eventBusProvider = configuration.GetValue<string>("EventBusProvider");

            if (string.IsNullOrEmpty(eventBusProvider))
            {
                throw new InvalidOperationException("EventBusProvider is not configured in appsettings.json");
            }

            Console.WriteLine($"Using event bus provider: {eventBusProvider}");

            if (eventBusProvider.Equals("NATS", StringComparison.OrdinalIgnoreCase))
            {
                services.AddSingleton<INatsConnection>(sp =>
                {
                    var natsUrl = configuration["Nats:Url"] ?? "nats://localhost:4222";
                    var options = NatsOpts.Default with { Url = natsUrl };
                    return new NatsConnection(options);
                });
                services.AddSingleton<IEventPublisher, NatsEventPublisher>();
            }
            else if (eventBusProvider.Equals("Log", StringComparison.OrdinalIgnoreCase))
            {
                services.AddSingleton<IEventPublisher, LogEventPublisher>();
            }
            else
            {
                throw new NotSupportedException($"Event bus provider '{eventBusProvider}' is not supported.");
            }
        }
    }
}
