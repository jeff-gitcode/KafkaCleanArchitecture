using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
using Infrastructure.Persistence;
using Domain.Entities;
using System;

namespace Infrastructure.Tests
{
    public class EntityRepositoryTest
    {
        private readonly AppDbContext _dbContext;
        private readonly EntityRepository _repository;

        public EntityRepositoryTest()
        {
            // Configure the in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new AppDbContext(options);
            _repository = new EntityRepository(_dbContext);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entity = new Entity { Id = entityId, Name = "Test Entity" };
            _dbContext.Entities.Add(entity);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(entityId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entityId, result.Id);
            Assert.Equal("Test Entity", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
        {
            // Arrange
            var entityId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByIdAsync(entityId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEntities()
        {
            // Arrange
            _dbContext.Entities.RemoveRange(_dbContext.Entities);
            await _dbContext.SaveChangesAsync();
            var entities = new[]
            {
                new Entity { Id = Guid.NewGuid(), Name = "Entity 1" },
                new Entity { Id = Guid.NewGuid(), Name = "Entity 2" }
            };
            _dbContext.Entities.AddRange(entities);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task AddAsync_ShouldAddEntity()
        {
            // Arrange
            var entity = new Entity { Id = Guid.NewGuid(), Name = "New Entity" };

            // Act
            await _repository.AddAsync(entity);

            // Assert
            var result = await _dbContext.Entities.FindAsync(entity.Id);
            Assert.NotNull(result);
            Assert.Equal("New Entity", result.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateEntity()
        {
            // Arrange
            var entity = new Entity { Id = Guid.NewGuid(), Name = "Old Name" };
            _dbContext.Entities.Add(entity);
            await _dbContext.SaveChangesAsync();

            // Act
            entity.Name = "Updated Name";
            await _repository.UpdateAsync(entity);

            // Assert
            var result = await _dbContext.Entities.FindAsync(entity.Id);
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveEntity_WhenEntityExists()
        {
            // Arrange
            var entity = new Entity { Id = Guid.NewGuid(), Name = "Entity to Delete" };
            _dbContext.Entities.Add(entity);
            await _dbContext.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entity.Id);

            // Assert
            var result = await _dbContext.Entities.FindAsync(entity.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDoNothing_WhenEntityDoesNotExist()
        {
            // Arrange
            var entityId = Guid.NewGuid();

            // Act
            await _repository.DeleteAsync(entityId);

            // Assert
            var result = await _dbContext.Entities.FindAsync(entityId);
            Assert.Null(result); // No exception should occur
        }

        public void Dispose()
        {
            // Clear the database after each test
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}