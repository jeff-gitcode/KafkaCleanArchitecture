using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WebApi.Tests
{
    public class EntityControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly IServiceProvider _serviceProvider;

        public EntityControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _serviceProvider = factory.Services;
        }

        [Fact]
        public async Task CreateEntity_ShouldReturnCreatedEntity()
        {
            // Arrange
            var newEntity = new { Name = "Test Entity" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/entity", newEntity);

            // Assert
            response.EnsureSuccessStatusCode();
            var createdEntity = await response.Content.ReadFromJsonAsync<Entity>();
            Assert.NotNull(createdEntity);
            Assert.Equal("Test Entity", createdEntity.Name);
        }

        [Fact]
        public async Task GetEntity_ShouldReturnEntity_WhenEntityExists()
        {
            // Arrange
            var entity = new Entity { Id = Guid.NewGuid(), Name = "Existing Entity" };
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Entities.Add(entity);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/entity/{entity.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var returnedEntity = await response.Content.ReadFromJsonAsync<Entity>();
            Assert.NotNull(returnedEntity);
            Assert.Equal(entity.Id, returnedEntity.Id);
            Assert.Equal(entity.Name, returnedEntity.Name);
        }

        [Fact]
        public async Task GetEntity_ShouldReturnNotFound_WhenEntityDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/entity/{nonExistentId}");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAllEntities_ShouldReturnAllEntities()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Entities.AddRange(
                new Entity { Id = Guid.NewGuid(), Name = "Entity 1" },
                new Entity { Id = Guid.NewGuid(), Name = "Entity 2" }
            );
            await dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/entity");

            // Assert
            response.EnsureSuccessStatusCode();
            var entities = await response.Content.ReadFromJsonAsync<Entity[]>();
            Assert.NotNull(entities);
            Assert.Equal(2, entities.Length);
        }

        [Fact]
        public async Task DeleteEntity_ShouldRemoveEntity()
        {
            // Arrange
            var entity = new Entity { Id = Guid.NewGuid(), Name = "Entity to Delete" };
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Entities.Add(entity);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/entity/{entity.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            using var verifyScope = _serviceProvider.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var deletedEntity = await verifyDbContext.Entities.FindAsync(entity.Id);
            Assert.Null(deletedEntity);
        }
    }
}