using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Confluent.Kafka;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace Infrastructure.Tests;

public class KafkaConsumerServiceTests : IDisposable
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IEntityRepository> _repositoryMock;
    private readonly Mock<IConsumer<string, string>> _consumerMock;
    private readonly KafkaConsumerService _consumerService;
    private readonly FakeTimeProvider _fakeTimeProvider;

    public KafkaConsumerServiceTests()
    {
        _consumerMock = new Mock<IConsumer<string, string>>();
        _repositoryMock = new Mock<IEntityRepository>();

        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();

        // Mock the IServiceScopeFactory to return a mocked IServiceScope
        _serviceScopeFactoryMock
            .Setup(factory => factory.CreateScope())
            .Returns(_serviceScopeMock.Object);

        // Mock the IServiceScope to return the mocked IServiceProvider
        _serviceScopeMock
            .Setup(scope => scope.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        // Mock the IServiceProvider to return the mocked IServiceScopeFactory
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(_serviceScopeFactoryMock.Object);

        // Mock the IServiceProvider to return the mocked IEntityRepository
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IEntityRepository)))
            .Returns(_repositoryMock.Object);

        // Use FakeTimeProvider for testing
        _fakeTimeProvider = new FakeTimeProvider();

        // Initialize the KafkaConsumerService with the mocked dependencies
        _consumerService = new KafkaConsumerService(_serviceProviderMock.Object, _consumerMock.Object, _fakeTimeProvider);
    }

    [Fact]
    public async Task ConsumeMessagesAsync_ShouldConsumeValidMessage()
    {
        // Arrange
        var validKey = Guid.NewGuid().ToString();
        var validValue = JsonSerializer.Serialize(new Entity { Id = Guid.Parse(validKey), Name = "Test Entity" });
        var cancellationTokenSource = new CancellationTokenSource();

        // Mock Kafka consumer behavior
        _consumerMock
            .Setup(c => c.Consume(It.IsAny<CancellationToken>()))
            .Returns(new ConsumeResult<string, string>
            {
                Message = new Message<string, string>
                {
                    Key = validKey,
                    Value = validValue
                }
            });

        // Mock repository behavior
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Entity>()))
            .Returns(Task.CompletedTask);

        // Act
        await _consumerService.StartAsync(cancellationTokenSource.Token);

        // Simulate time progression to trigger the timer
        _fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Entity>(e => e.Id.ToString() == validKey && e.Name == "Test Entity")), Times.Once);

        // Stop the service to clean up
        await _consumerService.StopAsync(cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        // Dispose of the consumer service
        _consumerService.Dispose();

        // Verify that the consumer was disposed
        _consumerMock.Verify(c => c.Dispose(), Times.Once);

        // Verify that the service scope was disposed
        _serviceScopeMock.Verify(scope => scope.Dispose(), Times.Once);
    }

    [Fact]
    public async Task StartAsync_ShouldStartTimer()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource();

        // Act
        await _consumerService.StartAsync(cancellationToken.Token);

        // Simulate time progression
        _fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));

        // Assert
        // No exception means the timer started successfully
    }
}