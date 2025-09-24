
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;

namespace Auth.Api.Extensions
{
    public static class DataProtectionExtensions
    {
        public static IServiceCollection AddDataProtectionServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var certPath = Path.Combine(environment.ContentRootPath, "Certs", "dp-cert.pfx");
            var certPassword = configuration["DataProtection:CertificatePassword"];
            var keysPath = Path.Combine(environment.ContentRootPath, ".keys");

            var dataProtectionBuilder = services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

            if (File.Exists(certPath) && !string.IsNullOrEmpty(certPassword))
            {
                var certBytes = File.ReadAllBytes(certPath);
                var certificate = X509CertificateLoader.LoadPkcs12(certBytes, certPassword);
                dataProtectionBuilder.ProtectKeysWithCertificate(certificate);
            }

            return services;
        }
    }
}
