namespace Auth.Application.Commands;

public class LinkProviderCommand
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
