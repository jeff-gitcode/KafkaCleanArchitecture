using System;
using System.Threading.Tasks;
using Application.Commands;
using Application.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;
using Xunit;

namespace WebApi.Tests
{
    public class EntityControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly EntityController _controller;

        public EntityControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new EntityController(_mediatorMock.Object);
        }

        [Fact]
        public async Task CreateEntity_ShouldReturnCreatedAtActionResult_WithCreatedEntity()
        {
            // Arrange
            var command = new CreateEntityCommand { Name = "Test Entity" };
            var createdEntity = new Entity { Id = Guid.NewGuid(), Name = "Test Entity" }; // Use the correct type
            _mediatorMock
                .Setup(m => m.Send(command, default))
                .ReturnsAsync(createdEntity); // Return the correct type

            // Act
            var result = await _controller.CreateEntity(command);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetEntity), createdAtActionResult.ActionName);
            Assert.Equal(createdEntity, createdAtActionResult.Value);
        }

        [Fact]
        public async Task GetEntity_ShouldReturnOkObjectResult_WithEntity_WhenEntityExists()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var query = new GetEntityQuery { Id = entityId };
            var entity = new Entity { Id = entityId, Name = "Test Entity" };

            // Ensure the mediator mock returns the entity
            _mediatorMock
                .Setup(m => m.Send(It.Is<GetEntityQuery>(q => q.Id == entityId), default))
                .ReturnsAsync(entity);

            // Act
            var result = await _controller.GetEntity(entityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(entity, okResult.Value);
        }

        [Fact]
        public async Task GetEntity_ShouldReturnNotFoundResult_WhenEntityDoesNotExist()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var query = new GetEntityQuery { Id = entityId };
            _mediatorMock
                .Setup(m => m.Send(query, default))
                .ReturnsAsync((Entity)null); // Explicitly cast null to Entity

            // Act
            var result = await _controller.GetEntity(entityId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAllEntities_ShouldReturnOkObjectResult_WithEntities()
        {
            // Arrange
            var query = new GetAllEntitiesQuery();
            var entities = new[]
            {
                new Entity { Id = Guid.NewGuid(), Name = "Entity 1" },
                new Entity { Id = Guid.NewGuid(), Name = "Entity 2" }
            };

            // Ensure the mediator mock returns the entities
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllEntitiesQuery>(), default))
                .ReturnsAsync(entities);

            // Act
            var result = await _controller.GetAllEntities();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(entities, okResult.Value);
        }

        [Fact]
        public async Task DeleteEntity_ShouldReturnNoContentResult()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var command = new DeleteEntityCommand { Id = entityId };
            _mediatorMock
                .Setup(m => m.Send(command, default))
                .ReturnsAsync(Unit.Value); // Return Task.FromResult(Unit.Value)

            // Act
            var result = await _controller.DeleteEntity(entityId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
