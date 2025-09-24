using Auth.Application.Interfaces;

namespace Auth.Application.Factories
{
    public interface IOAuthProviderFactory
    {
        IOAuthProviderService GetProvider(string provider);
    }
}
