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

            var projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.FullName;

            Console.WriteLine($"Project root directory: {projectRoot}");

            _config = new ConsumerConfig
            {
                // BootstrapServers = "localhost:9092", // Replace with your Kafka server address
                // GroupId = "consumer-group",
                // AutoOffsetReset = AutoOffsetReset.Earliest
                BootstrapServers = "localhost:9093",
                GroupId = "consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                SecurityProtocol = SecurityProtocol.Ssl,
                SslCaLocation = projectRoot + "/kafka-secrets/kafka.crt",
                SslCertificateLocation = projectRoot + "/kafka-secrets/kafka.crt", // Path to the client certificate in PEM format
                SslKeyLocation = projectRoot + "/kafka-secrets/kafka.key", // Path to the private key in PEM format
                SslKeyPassword = "kafka123"
            };
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Set the timer to trigger every 10 seconds
            // Set the timer to trigger every 10 seconds
            _timer = new Timer(async state => await ConsumeMessagesAsync(cancellationToken), null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);
            return Task.CompletedTask;
        }

        private async Task ConsumeMessagesAsync(CancellationToken cancellationToken)
        {
            using (var consumer = new ConsumerBuilder<string, string>(_config).Build())
            {
                consumer.Subscribe(_topic);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<IEntityRepository>();

                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)  // Run indefinitely
                        {
                            var cr = consumer.Consume(cancellationToken); // Poll for messages
                            if (cr != null)
                            {
                                Console.WriteLine($"Consumed message with key: {cr.Message.Key}, value: {cr.Message.Value}");

                                if (Guid.TryParse(cr.Message.Key, out var entityId))
                                {
                                    // Deserialize the message value into an Entity object
                                    var entity = System.Text.Json.JsonSerializer.Deserialize<Domain.Entities.Entity>(cr.Message.Value);
                                    if (entity != null)
                                    {
                                        entity.Id = entityId; // Ensure the ID from the key is set
                                        await repository.AddAsync(entity); // Save to the database asynchronously
                                        Console.WriteLine($"Entity with ID {entity.Id} and Name '{entity.Name}' saved to the database.");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Failed to deserialize message value: {cr.Message.Value}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Invalid entity ID: {cr.Message.Key}");
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Message consumption canceled.");
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