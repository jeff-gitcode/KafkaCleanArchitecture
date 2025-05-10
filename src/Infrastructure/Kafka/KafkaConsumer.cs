using Application.Interfaces;
using Confluent.Kafka;
using Domain.Entities;
using Infrastructure.Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Kafka
{
    public class KafkaConsumer
    {
        private readonly string _topic;
        private readonly ConsumerConfig _config;
        private readonly IServiceProvider _serviceProvider;

        public KafkaConsumer(string topic, string bootstrapServers, IServiceProvider serviceProvider)
        {
            _topic = topic;
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)); ;


            _config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = "consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        }

        public async Task StartConsuming(CancellationToken cancellationToken)
        {
            using (var consumer = new ConsumerBuilder<string, string>(_config).Build())
            {
                consumer.Subscribe(_topic);

                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var cr = consumer.Consume(cancellationToken);
                        await ProcessMessage(cr.Message.Key, cr.Message.Value);
                    }
                }
                catch (OperationCanceledException)
                {
                    consumer.Close();
                }
            }
        }

        private async Task ProcessMessage(string key, string value)
        {
            Console.WriteLine($"Consumed message with key: {key}, value: {value}");

            using (var scope = _serviceProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IEntityRepository>();

                try
                {
                    if (Guid.TryParse(key, out var entityId))
                    {
                        // Deserialize the value into an Entity object
                        var entity = System.Text.Json.JsonSerializer.Deserialize<Entity>(value);

                        if (entity != null)
                        {
                            entity.Id = entityId; // Ensure the ID from the key is set
                            // await repository.AddAsync(entity);
                            Console.WriteLine($"Entity with ID {entity.Id} and Name '{entity.Name}' saved to the database.");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to deserialize message value: {value}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid entity ID: {key}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                }
            }
        }
    }
}