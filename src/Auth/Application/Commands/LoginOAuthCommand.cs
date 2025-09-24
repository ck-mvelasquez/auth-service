
namespace Auth.Application.Commands
{
    public class LoginOAuthCommand
    {
        public string Provider { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
