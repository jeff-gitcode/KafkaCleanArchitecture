using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using FluentValidation;
using Application.Behaviors;

namespace Application
{
    public static class DI
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register MediatR for commands and queries
            services.AddMediatR(typeof(DI).Assembly);

            // Register FluentValidation validators
            services.AddValidatorsFromAssembly(typeof(DI).Assembly);

            // Add ValidationBehavior to MediatR pipeline
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}