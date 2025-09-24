
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Events;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Commands.Handlers
{
    public class ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordResetTokenRepository tokenRepository,
        IPasswordResetTokenGenerator tokenGenerator,
        IEventPublisher eventPublisher,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPasswordResetTokenRepository _tokenRepository = tokenRepository;
        private readonly IPasswordResetTokenGenerator _tokenGenerator = tokenGenerator;
        private readonly IEventPublisher _eventPublisher = eventPublisher;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger = logger;

        public async Task Handle(ForgotPasswordCommand command)
        {
            var user = await _userRepository.GetByEmailAsync(command.Email);
            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent user: {Email}", command.Email);
                return;
            }

            var token = _tokenGenerator.GenerateToken();
            var resetToken = new PasswordResetToken
            {
                Email = command.Email,
                Token = token,
                ExpirationDate = DateTime.UtcNow.AddHours(1)
            };

            await _tokenRepository.AddAsync(resetToken);
            _logger.LogInformation("Generated password reset token for user: {Email}", command.Email);

            // Correctly include the token in the event
            var passwordResetEvent = new PasswordResetRequestedEvent(user.Email, token);
            await _eventPublisher.PublishAsync(passwordResetEvent);
            _logger.LogInformation("Published PasswordResetRequestedEvent for user: {Email}", command.Email);
        }
    }
}
