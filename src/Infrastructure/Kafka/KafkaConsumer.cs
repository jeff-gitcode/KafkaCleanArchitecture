using Confluent.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Kafka
{
    public class KafkaConsumer
    {
        private readonly string _topic;
        private readonly ConsumerConfig _config;

        public KafkaConsumer(string topic, string bootstrapServers)
        {
            _topic = topic;
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
                        ProcessMessage(cr.Message.Key, cr.Message.Value);
                    }
                }
                catch (OperationCanceledException)
                {
                    consumer.Close();
                }
            }
        }

        private void ProcessMessage(string key, string value)
        {
            // Implement message processing logic here
            Console.WriteLine($"Consumed message with key: {key}, value: {value}");
        }
    }
}