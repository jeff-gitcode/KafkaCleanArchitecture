using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Kafka;
using Infrastructure.Persistence;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Confluent.Kafka;

namespace Infrastructure
{
    public static class DI
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString, string kafkaBootstrapServers)
        {
            services.AddSingleton(TimeProvider.System);

            // Register Kafka producer and consumer
            services.AddScoped<IKafkaProducer>(provider => new KafkaProducer(kafkaBootstrapServers));

            // Register Kafka consumer
            services.AddSingleton<IConsumer<string, string>>(provider =>
            {
                var projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.FullName;

                var config = new ConsumerConfig
                {
                    BootstrapServers = kafkaBootstrapServers,
                    GroupId = "consumer-group",
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    SecurityProtocol = SecurityProtocol.Ssl,
                    SslCaLocation = projectRoot + "/kafka-secrets/kafka.crt",
                    SslCertificateLocation = projectRoot + "/kafka-secrets/kafka.crt",
                    SslKeyLocation = projectRoot + "/kafka-secrets/kafka.key",
                    SslKeyPassword = "kafka123"
                };

                return new ConsumerBuilder<string, string>(config).Build();
            });

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