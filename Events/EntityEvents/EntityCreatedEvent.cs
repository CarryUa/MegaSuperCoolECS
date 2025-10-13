namespace ECS.Events.EntityEvents;

public class EntityCreatedEvent : Event
{
    public int EntityId { get; }

    public EntityCreatedEvent(int entityId)
    {
        EntityId = entityId;
    }
}