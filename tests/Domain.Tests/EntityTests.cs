using Xunit;
using Domain.Entities;

namespace Domain.Tests
{
    public class EntityTests
    {
        [Fact]
        public void Entity_Should_Set_Properties_Correctly()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entityName = "Test Entity";

            // Act
            var entity = new Entity
            {
                Id = entityId,
                Name = entityName
            };

            // Assert
            Assert.Equal(entityId, entity.Id);
            Assert.Equal(entityName, entity.Name);
        }
    }
}