using Auth.Application.Interfaces;
using Auth.Domain.Events;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Auth.Infrastructure.EventBus
{
    public class LogEventPublisher(ILogger<LogEventPublisher> logger) : IEventPublisher
    {
        private readonly ILogger<LogEventPublisher> _logger = logger;

        public Task PublishAsync(DomainEvent domainEvent)
        {
            var message = JsonSerializer.Serialize(domainEvent);
            _logger.LogInformation("Publishing event {EventName}: {Message}", domainEvent.GetType().Name, message);
            return Task.CompletedTask;
        }
    }
}
