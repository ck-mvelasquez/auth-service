using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Events;
using Auth.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Commands.Handlers;

public class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IEventPublisher eventPublisher,
    ILogger<RegisterUserCommandHandler> logger)
{
    public async Task Handle(RegisterUserCommand command)
    {
        logger.LogInformation("Attempting to register new user with email: {Email}", command.Email);

        var existingUser = await userRepository.GetByEmailAsync(command.Email);
        if (existingUser != null)
        {
            logger.LogWarning("Registration failed: User already exists with email: {Email}", command.Email);
            throw new Exception("User already exists.");
        }

        var passwordHash = passwordHasher.HashPassword(command.Password);
        var user = new User
        {
            Email = command.Email,
            PasswordHash = passwordHash,
        };

        await userRepository.AddAsync(user);

        await eventPublisher.PublishAsync(new UserRegisteredEvent(user.Id, user.Email));
        
        logger.LogInformation("User registered successfully with email: {Email}", command.Email);
    }
}
