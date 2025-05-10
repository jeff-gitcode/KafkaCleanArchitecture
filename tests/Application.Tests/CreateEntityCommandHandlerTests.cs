using Application.Commands;
using Application.Interfaces;
using Domain.Entities;
using Moq;

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
            _kafkaProducerMock.Verify(
                x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Entity>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenCommandIsInvalid()
        {
            // Arrange
            var command = new CreateEntityCommand { Name = "" }; // Invalid command
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _handler.Handle(command, cancellationToken));
        }
    }
}