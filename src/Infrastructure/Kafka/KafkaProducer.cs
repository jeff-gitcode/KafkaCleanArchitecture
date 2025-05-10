using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Application.Interfaces;
using System.Text.Json;

namespace Infrastructure.Kafka
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(string bootstrapServers)
        {
            var projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.FullName;

            Console.WriteLine($"Project root directory: {projectRoot}");

            var config = new ProducerConfig
            {
                // BootstrapServers = bootstrapServers,
                BootstrapServers = bootstrapServers,
                SecurityProtocol = SecurityProtocol.Ssl,
                SslCaLocation = projectRoot + "/kafka-secrets/kafka.crt",
                SslCertificateLocation = projectRoot + "/kafka-secrets/kafka.crt", // Path to the client certificate in PEM format
                SslKeyLocation = projectRoot + "/kafka-secrets/kafka.key", // Path to the private key in PEM format
                SslKeyPassword = "kafka123"
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceAsync<T>(string topic, string key, T value)
        {
            try
            {
                // Serialize the generic value to JSON
                Console.WriteLine($"Message sent to value: {value}");
                var serializedValue = JsonSerializer.Serialize(value);

                var result = await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = serializedValue });
                Console.WriteLine($"Message sent to topic {result.Topic} with offset {result.Offset}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error producing message: {ex.Message}");
            }
        }
    }
}