using Confluent.Kafka;
using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Kafka
{
    public class KafkaConsumerService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _topic = "entity-topic";
        private readonly ConsumerConfig _config;
        private Timer _timer;

        public KafkaConsumerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092", // Replace with your Kafka server address
                GroupId = "consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Set the timer to trigger every 10 seconds
            _timer = new Timer(ConsumeMessages, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        private void ConsumeMessages(object state)
        {
            using (var consumer = new ConsumerBuilder<string, string>(_config).Build())
            {
                consumer.Subscribe(_topic);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<IEntityRepository>();

                    try
                    {
                        var cr = consumer.Consume(TimeSpan.FromSeconds(5)); // Poll for messages
                        if (cr != null)
                        {
                            Console.WriteLine($"Consumed message with key: {cr.Message.Key}, value: {cr.Message.Value}");

                            if (Guid.TryParse(cr.Message.Key, out var entityId))
                            {
                                var entity = System.Text.Json.JsonSerializer.Deserialize<Domain.Entities.Entity>(cr.Message.Value);
                                if (entity != null)
                                {
                                    entity.Id = entityId;
                                    repository.AddAsync(entity).Wait();
                                    Console.WriteLine($"Entity with ID {entity.Id} and Name '{entity.Name}' saved to the database.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error consuming message: {ex.Message}");
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}