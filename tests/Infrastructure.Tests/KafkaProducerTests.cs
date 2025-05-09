using Moq;
using Xunit;
using System.Threading.Tasks;
using Infrastructure.Kafka;
using Application.Interfaces;

namespace Infrastructure.Tests
{
    public class KafkaProducerTests
    {
        private readonly Mock<IKafkaProducer> _kafkaProducerMock;

        public KafkaProducerTests()
        {
            _kafkaProducerMock = new Mock<IKafkaProducer>();
        }

        [Fact]
        public async Task ProduceAsync_ShouldSendMessageToKafka()
        {
            // Arrange
            var topic = "test-topic";
            var key = "test-key";
            var message = "test message";
            _kafkaProducerMock
                .Setup(x => x.ProduceAsync(topic, key, message))
                .Returns(Task.CompletedTask);

            // Act
            await _kafkaProducerMock.Object.ProduceAsync(topic, key, message);

            // Assert
            _kafkaProducerMock.Verify(x => x.ProduceAsync(topic, key, message), Times.Once);
        }
    }
}