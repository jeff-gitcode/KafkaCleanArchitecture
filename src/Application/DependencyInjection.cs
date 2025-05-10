using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;

namespace Application
{
    public static class DI
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register MediatR for commands and queries
            services.AddMediatR(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}