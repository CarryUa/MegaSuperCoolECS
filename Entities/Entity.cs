using ECS.Components;

namespace ECS.Entities;

public interface IEntity
{
    int Id { get; }
    List<IComponent> Components { get; }

    void AttachComponent(IComponent comp);
    void DetachComponent(IComponent comp);
};

public class Entity : IEntity
{
    public int Id { get; }
    public List<IComponent> Components { get; }

    private int _id;
    private List<IComponent> components = new List<IComponent>();

    public Entity(int id)
    {
        Id = id;
        Components = components;
    }

    public void AttachComponent(IComponent comp)
    {
    }
    public void DetachComponent(IComponent comp)
    {

    }

    public override string ToString()
    {
        return $"Entity {Id} with {Components.Count} components";
    }
};