using Microsoft.Extensions.DependencyInjection;

namespace Domain
{
    public static class DI
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            // Register domain-specific services here (if any)
            return services;
        }
    }
}