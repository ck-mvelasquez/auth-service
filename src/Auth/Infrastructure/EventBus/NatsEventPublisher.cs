
using Auth.Application.Interfaces;
using Auth.Domain.Events;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Net;
using System.Text.Json;

namespace Auth.Infrastructure.EventBus;

public class NatsEventPublisher(INatsConnection connection, ILogger<NatsEventPublisher> logger) : IEventPublisher
{
    private readonly INatsJSContext _jetStream = connection.CreateJetStreamContext();

    public async Task PublishAsync(DomainEvent domainEvent)
    {
        var subject = domainEvent.GetType().Name;
        
        var message = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
        var body = System.Text.Encoding.UTF8.GetBytes(message);

        logger.LogInformation("Publishing event {EventName} to NATS JetStream.", subject);
        
        var ack = await _jetStream.PublishAsync(subject, body);
        ack.EnsureSuccess();

        logger.LogInformation("Successfully published event {EventName} with sequence number {Seq}", subject, ack.Seq);
    }
}
