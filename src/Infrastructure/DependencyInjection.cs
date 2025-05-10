using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Kafka;
using Infrastructure.Persistence;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public static class DI
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString, string kafkaBootstrapServers)
        {
            // Register Kafka producer and consumer
            services.AddScoped<IKafkaProducer>(provider => new KafkaProducer(kafkaBootstrapServers));
            services.AddHostedService<KafkaConsumerService>();

            // Register Entity Framework Core with In-Memory Database
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(connectionString));

            // Register repositories
            services.AddScoped<IEntityRepository, EntityRepository>();

            services.AddHostedService<KafkaConsumerService>();

            return services;
        }
    }
}