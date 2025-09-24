
using NATS.Client.Core;
using Auth.Api.Extensions;
using Auth.Infrastructure.EventBus;

namespace Auth.Api.HostedServices;

public class NatsJetStreamSetupService(
    ILogger<NatsJetStreamSetupService> logger,
    IConfiguration configuration,
    INatsConnection natsConnection) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var eventBusProvider = configuration.GetValue<string>("EventBusProvider");
        if (!string.Equals(eventBusProvider, "NATS", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        logger.LogInformation("NATS JetStream setup service is starting.");

        await natsConnection.EnsureStreamExistsAsync(
            NatsConstants.StreamName,
            NatsConstants.Subjects,
            logger: logger,
            cancellationToken: cancellationToken);

        logger.LogInformation("NATS JetStream stream '{StreamName}' is configured.", NatsConstants.StreamName);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("NATS JetStream setup service is stopping.");
        return Task.CompletedTask;
    }
}
