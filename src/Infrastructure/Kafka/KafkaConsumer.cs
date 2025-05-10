using System.Text.Json;
using Application.Interfaces;
using Confluent.Kafka;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class KafkaConsumerService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<string, string> _consumer;
    private readonly string _topic = "entity-topic";
    private readonly TimeProvider _timeProvider;
    private ITimer? _timer;

    public KafkaConsumerService(IServiceProvider serviceProvider, IConsumer<string, string> consumer, TimeProvider timeProvider)
    {
        _serviceProvider = serviceProvider;
        _consumer = consumer;
        _timeProvider = timeProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = _timeProvider.CreateTimer(
            callback: async _ => await ConsumeMessagesAsync(cancellationToken),
            state: null,
            dueTime: TimeSpan.Zero,
            period: Timeout.InfiniteTimeSpan
            );

        // _timer = new Timer(async state => await ConsumeMessagesAsync(cancellationToken), null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        return Task.CompletedTask;
    }

    private async Task ConsumeMessagesAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(cancellationToken);

                if (consumeResult != null)
                {
                    var entity = JsonSerializer.Deserialize<Entity>(consumeResult.Message.Value);
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetRequiredService<IEntityRepository>();
                        await repository.AddAsync(entity);
                        Console.WriteLine($"Consumed message '{consumeResult.Value}' at: '{consumeResult.TopicPartitionOffset}'. Entity ID: {entity.Id}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error consuming message: {ex.Message}");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        // _timer?.Change(Timeout.InfiniteTimeSpan, TimeSpan.Zero);
        // _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // _timer?.Dispose();
        _consumer.Close();
        _consumer.Dispose();
    }
}