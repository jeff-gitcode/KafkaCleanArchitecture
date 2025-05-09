using System.Threading;
using System.Threading.Tasks;
using Application.Commands;
using Application.Interfaces;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.Tests
{
    public class CreateEntityCommandHandlerTests
    {
        private readonly Mock<IKafkaProducer> _kafkaProducerMock;
        private readonly CreateEntityCommandHandler _handler;

        public CreateEntityCommandHandlerTests()
        {
            _kafkaProducerMock = new Mock<IKafkaProducer>();
            _handler = new CreateEntityCommandHandler(_kafkaProducerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldProduceMessage_WhenCommandIsValid()
        {
            // Arrange
            var command = new CreateEntityCommand { Name = "Test Entity" };
            var cancellationToken = CancellationToken.None;

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _kafkaProducerMock.Verify(x => x.ProduceAsync("test-topic", "test-key", It.IsAny<Entity>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenCommandIsInvalid()
        {
            // Arrange
            var command = new CreateEntityCommand { Name = null }; // Invalid command
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, cancellationToken));
        }
    }
}