namespace Domain.Entities;

public class EntityCreatedEvent
{
    public Guid EntityId { get; }
    public string EntityName { get; }
    public DateTime CreatedAt { get; }

    public EntityCreatedEvent(Guid entityId, string entityName)
    {
        EntityId = entityId;
        EntityName = entityName;
        CreatedAt = DateTime.UtcNow;
    }
}