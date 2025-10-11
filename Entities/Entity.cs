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
    private List<IComponent> _components = new List<IComponent>();

    public string Name { get; set; } = "";

    public Entity(int id)
    {
        Id = id;
        Components = _components;
    }

    public void AttachComponent(IComponent comp)
    {
        if (!_components.Contains(comp))
        {
            _components.Add(comp);
        }
    }
    public void DetachComponent(IComponent comp)
    {
        if (_components.Contains(comp))
        {
            _components.Remove(comp);
        }
    }

    public override string ToString()
    {
        return $"Entity {Id} with {Components.Count} components";
    }
};